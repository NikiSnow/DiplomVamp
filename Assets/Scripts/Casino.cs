using System.Collections;
using UnityEngine;

public class Casino : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform spinner;
    [SerializeField] private float speed = 2500f;

    [Header("Positions")]
    [SerializeField] private Vector3 startPos = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 topPos = new Vector3(0f, 500f, 0f);
    [SerializeField] private Vector3 downPos = new Vector3(0f, -500f, 0f);

    [Header("Timing")]
    [SerializeField] private float stopDelay = 3.5f;

    [Header("Blocks")]
    [SerializeField] private float amountOfBlocks = 3f;
    [SerializeField] private RectTransform block;

    [Header("Stop Positions")]
    [SerializeField] private float bluePos = 0f;
    [SerializeField] private float cyanPos = 100f;
    [SerializeField] private float purplePos = 200f;

    [Header("Probabilities (LossR < Winx2 < Winx3)")]
    [SerializeField] private float lossR = 0.425f;
    [SerializeField] private float winX2 = 0.7f;
    [SerializeField] private float winX3 = 0.9f;

    private float startTime;
    private float journeyLength;
    private bool isSpinning = false;
    private bool shouldStop = false;
    private bool isStopped = false;
    private float stopTime;
    private Vector3 targetPosition;
    private System.Random random = new System.Random();

    private void Start()
    {
        // Инициализация позиций на основе размера блока
        float blockHeight = block != null ? block.rect.width : 100f;
        topPos = new Vector3(spinner.anchoredPosition.x, blockHeight * (amountOfBlocks / 2f), 0f);
        startPos = new Vector3(spinner.anchoredPosition.x, 0f, 0f);
        downPos = new Vector3(spinner.anchoredPosition.x, -(blockHeight * (amountOfBlocks / 2f)), 0f);
    }

    private void OnEnable()
    {
        StartGambling();
    }

    public void StartGambling()
    {
        Time.timeScale = 0f;
        AudioListener.pause = true;

        panel.SetActive(true);
        ResetSpinner();

        targetPosition = GetRandomTargetPosition();
        StartCoroutine(StopRoutine());

        isSpinning = true;
    }

    private void ResetSpinner()
    {
        spinner.anchoredPosition = startPos;
        startTime = Time.unscaledTime;
        journeyLength = Vector3.Distance(downPos, topPos);

        shouldStop = false;
        isStopped = false;
        isSpinning = true;
    }

    private Vector3 GetRandomTargetPosition()
    {
        float randomValue = (float)random.NextDouble();
        float targetOffset;

        if (randomValue < lossR)
        {
            // Проигрыш - случайная комбинация
            targetOffset = GetRandomOffset();
            Debug.Log("Loss");
        }
        else if (randomValue < winX2)
        {
            targetOffset = cyanPos;
            Debug.Log("Win x2");
        }
        else if (randomValue < winX3)
        {
            targetOffset = bluePos;
            Debug.Log("Win x3");
        }
        else
        {
            targetOffset = purplePos;
            Debug.Log("Win x5");
        }

        return new Vector3(0f, downPos.y + targetOffset, 0f);
    }

    private float GetRandomOffset()
    {
        float randomValue = (float)random.NextDouble();

        if (randomValue < 0.33f)
            return cyanPos;
        if (randomValue < 0.66f)
            return bluePos;
        return purplePos;
    }

    private IEnumerator StopRoutine()
    {
        yield return new WaitForSecondsRealtime(stopDelay);
        shouldStop = true;
        stopTime = Time.unscaledTime;
    }

    private void Update()
    {
        if (!isSpinning) return;

        float currentTime = Time.unscaledTime;
        float distCovered = (currentTime - startTime) * speed;
        float journeyProgress = distCovered / journeyLength;

        if (!shouldStop)
        {
            // Движение вверх-вниз до получения сигнала остановки
            Vector3 currentPos = Vector3.Lerp(downPos, topPos, journeyProgress);
            spinner.anchoredPosition = currentPos;

            // Сброс цикла при достижении вершины
            if (journeyProgress >= 1f)
            {
                startTime = currentTime;
            }
        }
        else if (!isStopped)
        {
            // Плавная остановка на целевой позиции
            float stopDistance = Vector3.Distance(targetPosition, topPos);
            float stopDistCovered = (currentTime - stopTime) * speed;
            float stopProgress = stopDistCovered / stopDistance;

            spinner.anchoredPosition = Vector3.Lerp(targetPosition, topPos, stopProgress);

            if (stopProgress >= 1f)
            {
                isStopped = true;
                isSpinning = false;
                spinner.anchoredPosition = targetPosition;

                // Здесь можно добавить логику показа результата
                OnSpinnerStopped();
            }
        }
    }

    private void OnSpinnerStopped()
    {
        // Определяем выпавший результат
        float yOffset = spinner.anchoredPosition.y - downPos.y;
        string result = GetResultFromOffset(yOffset);
        Debug.Log($"Spinner stopped on: {result}");

        // Здесь можно вызвать событие для UI или другой логики
        // Например: OnGamblingComplete?.Invoke(result);
    }

    private string GetResultFromOffset(float offset)
    {
        if (Mathf.Approximately(offset, cyanPos))
            return "Cyan";
        if (Mathf.Approximately(offset, bluePos))
            return "Blue";
        if (Mathf.Approximately(offset, purplePos))
            return "Purple";
        return "Unknown";
    }

    private void OnValidate()
    {
        // Валидация вероятностей
        if (lossR < 0f || lossR > 1f)
            lossR = Mathf.Clamp01(lossR);
        if (winX2 < lossR || winX2 > 1f)
            winX2 = Mathf.Clamp(winX2, lossR, 1f);
        if (winX3 < winX2 || winX3 > 1f)
            winX3 = Mathf.Clamp(winX3, winX2, 1f);
    }
}