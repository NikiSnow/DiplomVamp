using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casino : MonoBehaviour
{

    [SerializeField] GameObject ObjPlayer;
    [SerializeField] Player ThePlayer;
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform spinner;
    [SerializeField] private float speed = 2500f;
    [SerializeField] float StoppingSpeed = 50f;

    [Header("Positions")]
    [SerializeField] private Vector3 startPos = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 topPos = new Vector3(0f, 500f, 0f);
    [SerializeField] private Vector3 downPos = new Vector3(0f, -500f, 0f);
    [SerializeField] float Padding = 20f;

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

    [SerializeField] int TempMultip = 1;
    [SerializeField] private List<GameObject> ChildSecrets;

    [Header("PassiveRewards")]
    [Header("WhitePassiveRewards")]
    [SerializeField] int WhiteAddArmor = 1;
    [SerializeField] int WhiteAddHealthRegen = 1;
    [Header("GreenPassiveRewards")]
    [SerializeField] int GreenAddArmor = 3;
    [SerializeField] int GreenAddHealthRegen = 2;
    [SerializeField] int GreenAddMaxHp = 15;
    [SerializeField] int GreenAddDmg = 5;
    [Header("BluePassiveRewards")]
    [SerializeField] int BlueAddArmor = 5;
    [SerializeField] int BlueAddHealthRegen = 5;
    [SerializeField] int BlueAddMaxHp = 25;
    [SerializeField] int BlueAddDmg = 15;
    [SerializeField] float BlueAddAttackScale = 0.1f;
    [SerializeField] int BlueAddSpeed = 1;
    [Header("PrefabRewards")]
    [SerializeField] private List<GameObject> PurpleRewardPrefabs;
    [SerializeField] private List<GameObject> YellowRewardPrefabs;

    private float startTime;
    private float journeyLength;
    private bool isSpinning = false;
    private bool shouldStop = false;
    private bool isStopped = false;
    private float stopTime;
    private Vector3 targetPosition;
    private System.Random random = new System.Random();

    private Vector3 stopStartPosition;
    private float stopStartTime;
    private float stopStartDistance;
    private bool hasFixedStartPosition = false;


    float HalfBlock;

    private void Start()
    {

        // Инициализация позиций на основе размера блока
        float blockHeight = block != null ? block.rect.width : 100f;
        HalfBlock = blockHeight / 2;
        //Debug.Log(blockHeight);
        //Debug.Log(HalfBlock);
        topPos = new Vector3(spinner.anchoredPosition.x, (blockHeight + Padding) * (amountOfBlocks / 2f), 0f);
        startPos = new Vector3(spinner.anchoredPosition.x, 0f, 0f);
        downPos = new Vector3(spinner.anchoredPosition.x, -((blockHeight + Padding) * (amountOfBlocks / 2f)), 0f);
        //StartGambling();
    }

    public void StartGambling()
    {
        AudioListener.pause = true;

        for(int i = 0; i < ChildSecrets.Count; i++)
        {
            ChildSecrets[i].SetActive(false);
        }

        panel.SetActive(true);
        ResetSpinner();

        targetPosition = GetRandomTargetPosition();
        StartCoroutine(StopRoutine());

        isSpinning = true;
        Time.timeScale = 0f;
    }

    private void ResetSpinner()
    {
        spinner.anchoredPosition = startPos;
        startTime = Time.unscaledTime;
        journeyLength = Vector3.Distance(downPos, topPos);

        shouldStop = false;
        isStopped = false;
        isSpinning = true;

        hasFixedStartPosition = false;
    }

    private Vector3 GetRandomTargetPosition()
    {
        double r = random.NextDouble();
        //Zero = 2 item
        if(r < 0.65) //white
        {
            TempMultip = 2; //3
            ChildSecrets[2].SetActive(true);
            Debug.Log("White");
        }
        else if (r < 0.85) //Green
        {
            TempMultip = 6;
            ChildSecrets[4].SetActive(true);
            Debug.Log("Green");
        }
        else if (r < 0.95) //Blue
        {
            TempMultip = 8;
            ChildSecrets[5].SetActive(true);
            Debug.Log("Blue");
        }
        else if (r < 0.98) //Purple
        {
            TempMultip = 12;
            ChildSecrets[7].SetActive(true);
            Debug.Log("Purple");
        }
        else if (r < 1) //Yellow
        {
            TempMultip = 14;
            ChildSecrets[8].SetActive(true);
            Debug.Log("Yellow");
        }


            return new Vector3(0f, downPos.y + (HalfBlock * TempMultip), 0f);
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
            if (!hasFixedStartPosition)
            {
                stopStartPosition = spinner.anchoredPosition;
                stopStartTime = currentTime;
                stopStartDistance = Vector3.Distance(targetPosition, stopStartPosition);
                hasFixedStartPosition = true;
            }

            // Вычисляем прогресс остановки
            float stopDistCovered = (currentTime - stopStartTime) * StoppingSpeed;
            float stopProgress = stopDistCovered / stopStartDistance;
            stopProgress = Mathf.Clamp01(stopProgress);

            // Плавно двигаемся от начальной позиции к целевой
            spinner.anchoredPosition = Vector3.Lerp(downPos, targetPosition, stopProgress);

            // Проверяем, завершилась ли остановка
            if (stopProgress >= 1f)
            {
                isStopped = true;
                isSpinning = false;
                spinner.anchoredPosition = targetPosition;
                OnSpinnerStopped();
            }
        }
    }

    private void OnSpinnerStopped()
    {
        double r = random.NextDouble();
        if (TempMultip == 2) //White
        {
            if (r < 0.5)
            {
                ThePlayer.GiveArmor(WhiteAddArmor);
            }
            else
            {
                ThePlayer.GiveHPRegen(WhiteAddHealthRegen);
            }
        }
        else if (TempMultip == 6) //Green
        {
            if (r < 0.25)
            {
                ThePlayer.GiveArmor(GreenAddArmor);
            }
            else if(r < 0.5)
            {
                ThePlayer.GiveHPRegen(GreenAddHealthRegen);
            }
            else if (r < 0.75)
            {
                ThePlayer.GiveMaxHp(GreenAddMaxHp);
            }
            else if (r < 1)
            {
                ThePlayer.GiveDmg(GreenAddDmg);
            }
        }
        else if (TempMultip == 8) //Blue
        {
            if (r < 0.25)
            {
                ThePlayer.GiveArmor(BlueAddArmor);
            }
            else if (r < 0.5)
            {
                ThePlayer.GiveHPRegen(BlueAddHealthRegen);
            }
            else if (r < 0.75)
            {
                ThePlayer.GiveMaxHp(BlueAddMaxHp);
            }
            else if (r < 1)
            {
                ThePlayer.GiveDmg(BlueAddDmg);
            }
            else if (r < 1)
            {
                ThePlayer.GiveAttackScale(BlueAddAttackScale);
            }
            else if (r < 1)
            {
                ThePlayer.GiveSpeed(BlueAddSpeed);
            }

        }
        else if (TempMultip == 12) //Purple
        {
            GameObject newReward = Instantiate(PurpleRewardPrefabs[0]);
            newReward.GetComponent<PurpleReward>();
        }
        else if (TempMultip == 14)
        {
            GameObject newReward = Instantiate(YellowRewardPrefabs[0]);
            newReward.GetComponent<YellowReward>();
        }
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

    public void ContinueBut()
    {
        ThePlayer.ChestRewardPanel.SetActive(false);
        AudioListener.pause = false;

        panel.SetActive(false);

        Time.timeScale = 1f;
    }
}