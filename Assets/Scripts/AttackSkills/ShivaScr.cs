using System.Collections;
using UnityEngine;

public class ShivaScr : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerNode;
    [SerializeField] private CircleCollider2D coll;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float startScale = 1f;
    [SerializeField] private float endScale = 10f;

    [Header("Damage Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float slowing = 0.9f;
    [SerializeField] private float additionalAttack = 0.1f;

    [SerializeField] YellowReward MyParent;

    private bool isAnimating = false;
    private float elapsedTime = 0f;
    private float currentTargetScale = 1f;
    private float currentStartScale = 1f;


    private void OnEnable()
    {
        //transform.localScale = Vector3.one * startScale;
        StartScaleAnimation(endScale, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что коллайдер - триггер
        if (!coll.isTrigger) return;

        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
        {
            // Fallback если bufferScr не назначен
            enemy.TakeDmg(10f);
        }
    }

    private void StartScaleAnimation(float targetScale, float animDuration)
    {
        // Эмитируем событие (если нужна система событий)
        // EventManager.Instance.Emit("ShivaRefresh");

        if (isAnimating) return;

        StartCoroutine(ScaleAnimationCoroutine(targetScale, animDuration));
    }

    IEnumerator ScaleAnimationCoroutine(float targetScale, float animDuration)
    {
        isAnimating = true;
        elapsedTime = 0f;
        currentStartScale = transform.localScale.x;
        currentTargetScale = targetScale;

        Debug.Log($"Start animation: scale {currentStartScale} -> {currentTargetScale}");

        while (elapsedTime < animDuration)
        {
            elapsedTime += Time.deltaTime;

            // Рассчитываем прогресс (0 -> 1)
            float t = Mathf.Clamp01(elapsedTime / animDuration);

            // Добавляем easeOutQuad эффект
            t = 1f - Mathf.Pow(1f - t, 2f);

            // Интерполируем масштаб
            float currentScale = Mathf.Lerp(currentStartScale, currentTargetScale, t);
            transform.localScale = new Vector3(currentScale, currentScale, 1f);

            //transform.position = Vector3.zero; // В Unity лучше использовать localPosition

            yield return null;
        }

        // Фиксируем конечные значения
        transform.localScale = new Vector3(currentTargetScale, currentTargetScale, 1f);
        StartCoroutine(MyParent.WaitCD());
        isAnimating = false;
        this.gameObject.SetActive(false);

        Debug.Log("Scale animation completed!");
    }
}
