using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerWaterEffect : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Player Player;

    [Header("Water Movement")]
    [Tooltip("Базовая скорость в воде. 0.8 = минус 20%")]
    [SerializeField] private float WaterMoveMultiplier = 0.8f;

    [Tooltip("Ускорение в воде")]
    [SerializeField] private float WaterAcceleration = 10f;

    [Tooltip("Торможение в воде. Чем меньше, тем сильнее скольжение")]
    [SerializeField] private float WaterDeceleration = 4f;

    [Header("Water Charge")]
    [Tooltip("За сколько секунд стояния эффект заполнится полностью")]
    [SerializeField] private float WaterFillTime = 5f;

    [Tooltip("Максимальный дополнительный штраф при полном заполнении")]
    [Range(0f, 0.95f)]
    [SerializeField] private float MaxWaterStartPenalty = 0.6f;

    [Tooltip("Скорость заполнения эффекта")]
    [SerializeField] private float WaterFillSpeedMultiplier = 1f;

    [Tooltip("Скорость отката эффекта")]
    [SerializeField] private float WaterDrainSpeedMultiplier = 1.2f;

    [Tooltip("Порог, ниже которого считаем, что игрок реально стоит")]
    [SerializeField] private float StillVelocityThreshold = 0.08f;

    [Header("Debug")]
    [SerializeField] private bool EnableDebugLogs = true;
    [SerializeField] private float DebugLogInterval = 0.35f;

    private bool isInWater = false;
    private float waterCharge = 0f;
    private float debugTimer = 0f;

    private enum WaterDebugState
    {
        None,
        OutsideWater,
        InWaterMoving,
        InWaterStanding
    }

    private WaterDebugState currentDebugState = WaterDebugState.None;
    private WaterDebugState previousDebugState = WaterDebugState.None;

    private void Reset()
    {
        Player = GetComponent<Player>();
    }

    private void Awake()
    {
        if (Player == null)
        {
            Player = GetComponent<Player>();
        }
    }

    private void Update()
    {
        UpdateWaterCharge();
        UpdateDebugState();
        HandleDebugLogs();
    }

    private void UpdateWaterCharge()
    {
        if (Player == null || Player.PlayerRb == null)
        {
            return;
        }

        bool hasInput = Player.HasMovementInput;
        bool isActuallyStill = Player.PlayerRb.linearVelocity.magnitude <= StillVelocityThreshold;

        if (isInWater)
        {
            if (!hasInput && isActuallyStill)
            {
                float fillRate = (1f / WaterFillTime) * WaterFillSpeedMultiplier;
                waterCharge += fillRate * Time.deltaTime;
            }
            else
            {
                float drainRate = (1f / WaterFillTime) * WaterDrainSpeedMultiplier;
                waterCharge -= drainRate * Time.deltaTime;
            }
        }
        else
        {
            float drainRate = (1f / WaterFillTime) * (WaterDrainSpeedMultiplier * 2f);
            waterCharge -= drainRate * Time.deltaTime;
        }

        waterCharge = Mathf.Clamp01(waterCharge);
    }

    private void UpdateDebugState()
    {
        previousDebugState = currentDebugState;

        if (!isInWater)
        {
            currentDebugState = WaterDebugState.OutsideWater;
            return;
        }

        bool hasInput = Player != null && Player.HasMovementInput;
        bool isActuallyStill = Player != null &&
                               Player.PlayerRb != null &&
                               Player.PlayerRb.linearVelocity.magnitude <= StillVelocityThreshold;

        if (!hasInput && isActuallyStill)
        {
            currentDebugState = WaterDebugState.InWaterStanding;
        }
        else
        {
            currentDebugState = WaterDebugState.InWaterMoving;
        }
    }

    private void HandleDebugLogs()
    {
        if (!EnableDebugLogs)
        {
            return;
        }

        debugTimer += Time.deltaTime;

        if (currentDebugState != previousDebugState)
        {
            switch (currentDebugState)
            {
                case WaterDebugState.OutsideWater:
                    Debug.Log("[Water] Игрок вышел из воды. Эффект воды снят.");
                    break;

                case WaterDebugState.InWaterMoving:
                    Debug.Log("[Water] Игрок в воде и двигается. Эффект спадает.");
                    break;

                case WaterDebugState.InWaterStanding:
                    Debug.Log("[Water] Игрок стоит в воде. Начинается накопление замедления.");
                    break;
            }

            debugTimer = 0f;
        }

        if (debugTimer >= DebugLogInterval)
        {
            float chargePercent = waterCharge * 100f;
            float speedMultiplier = GetCurrentMoveMultiplier();

            switch (currentDebugState)
            {
                case WaterDebugState.InWaterStanding:
                    Debug.Log("[Water] Стоит в воде | Накопление: " +
                              chargePercent.ToString("F0") +
                              "% | Текущий множитель скорости: " +
                              speedMultiplier.ToString("F2"));
                    break;

                case WaterDebugState.InWaterMoving:
                    Debug.Log("[Water] Движется в воде | Остаток эффекта: " +
                              chargePercent.ToString("F0") +
                              "% | Текущий множитель скорости: " +
                              speedMultiplier.ToString("F2"));
                    break;
            }

            debugTimer = 0f;
        }
    }

    public float GetCurrentMoveMultiplier()
    {
        if (!isInWater)
        {
            return 1f;
        }

        float extraPenalty = Mathf.Lerp(0f, MaxWaterStartPenalty, waterCharge);
        float finalMultiplier = WaterMoveMultiplier * (1f - extraPenalty);

        return Mathf.Clamp(finalMultiplier, 0.15f, 1f);
    }

    public float GetCurrentAcceleration(float defaultAcceleration)
    {
        if (!isInWater)
        {
            return defaultAcceleration;
        }

        return WaterAcceleration;
    }

    public float GetCurrentDeceleration(float defaultDeceleration)
    {
        if (!isInWater)
        {
            return defaultDeceleration;
        }

        return WaterDeceleration;
    }

    public bool IsInWater()
    {
        return isInWater;
    }

    public float GetWaterCharge()
    {
        return waterCharge;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            isInWater = true;

            if (EnableDebugLogs)
            {
                Debug.Log("[Water] Игрок вошёл в воду. Базовое замедление активно.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Water"))
        {
            isInWater = false;

            if (EnableDebugLogs)
            {
                Debug.Log("[Water] Игрок покинул воду.");
            }
        }
    }
}