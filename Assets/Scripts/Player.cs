using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Rigidbody2D Rb;
    [SerializeField] private SpriteRenderer PlayerSprite;
    [SerializeField] private PlayerWaterEffect WaterEffect;
    [SerializeField] private SurfaceInteractor SurfaceInteractor;

    [Header("Movement")]
    [SerializeField] private float Speed = 5f;
    [SerializeField] private float GroundAcceleration = 22f;
    [SerializeField] private float GroundDeceleration = 28f;

    [Header("Health")]
    [SerializeField] private float MaxHp = 100f;
    [SerializeField] private float CurrHp = 100f;

    [Header("Progress")]
    [SerializeField] private LevelManager LevelMan;

    [Header("UI")]
    [SerializeField] private Image HealthVisual;

    [Header("PassiveAdds")]
    [SerializeField] private int Armor = 0; // White
    [SerializeField] private int HealthRegen = 0; // White
    [SerializeField] private int AddMaxHp = 0; // Green
    [SerializeField] private int AddDmg = 0; // Green
    [SerializeField] private float AddAttackScale = 1f; // Blue
    [SerializeField] private int AddSpeed = 0; // Blue

    [SerializeField] public GameObject ChestRewardPanel;
    [SerializeField] TMP_Text ChestRewardText;

    [Header("Invulnerability")]
    [SerializeField] private bool ShowInvulnerabilityDebug = false;

    private float hor = 0f;
    private float ver = 0f;

    private Vector2 input;
    private Vector2 targetVelocity;

    private bool movementLocked = false;
    private float invulnerabilityTimer = 0f;

    public Vector2 InputVector => input;
    public bool HasMovementInput => input.sqrMagnitude > 0.0001f;
    public Rigidbody2D PlayerRb => Rb;

    public float BaseSpeed => Speed;
    public float CurrentMaxHp => MaxHp + AddMaxHp;

    public int CurrentArmor => Armor;
    public int CurrentHealthRegen => HealthRegen;
    public int CurrentAddMaxHp => AddMaxHp;
    public int CurrentAddDmg => AddDmg;
    public float CurrentAddAttackScale => AddAttackScale;
    public int CurrentAddSpeed => AddSpeed;

    public bool IsInvulnerable => invulnerabilityTimer > 0f;
    public float InvulnerabilityRemaining => Mathf.Max(0f, invulnerabilityTimer);

    public string CurrentSurfaceName
    {
        get
        {
            if (SurfaceInteractor != null && SurfaceInteractor.HasActiveSurface)
            {
                return SurfaceInteractor.CurrentSurfaceName;
            }

            if (WaterEffect != null && WaterEffect.IsInWater())
            {
                return "Water";
            }

            return "Normal";
        }
    }

    public float CurrentSpeedMultiplier
    {
        get
        {
            if (SurfaceInteractor != null && SurfaceInteractor.HasActiveSurface)
            {
                return SurfaceInteractor.CurrentSpeedMultiplier;
            }

            if (WaterEffect != null)
            {
                return WaterEffect.GetCurrentMoveMultiplier();
            }

            return 1f;
        }
    }

    public float CurrentDashMultiplier
    {
        get
        {
            if (SurfaceInteractor != null && SurfaceInteractor.HasActiveSurface)
            {
                return SurfaceInteractor.CurrentDashMultiplier;
            }

            return 1f;
        }
    }

    public SurfaceInteractor CurrentSurfaceInteractor => SurfaceInteractor;

    private void Reset()
    {
        Rb = GetComponent<Rigidbody2D>();
        PlayerSprite = GetComponentInChildren<SpriteRenderer>();
        WaterEffect = GetComponent<PlayerWaterEffect>();
        SurfaceInteractor = GetComponent<SurfaceInteractor>();
        LevelMan = FindObjectOfType<LevelManager>();
    }

    private void Awake()
    {
        if (Rb == null)
        {
            Rb = GetComponent<Rigidbody2D>();
        }

        if (PlayerSprite == null)
        {
            PlayerSprite = GetComponentInChildren<SpriteRenderer>();
        }

        if (WaterEffect == null)
        {
            WaterEffect = GetComponent<PlayerWaterEffect>();
        }

        if (SurfaceInteractor == null)
        {
            SurfaceInteractor = GetComponent<SurfaceInteractor>();
        }

        if (LevelMan == null)
        {
            LevelMan = FindObjectOfType<LevelManager>();
        }

        CurrHp = Mathf.Clamp(CurrHp, 0f, CurrentMaxHp);
        UpdateHealthVisual();
    }

    private void Update()
    {
        ReadInput();
        UpdateFlip();
        UpdateInvulnerability();
    }

    private void FixedUpdate()
    {
        if (movementLocked)
        {
            return;
        }

        Move();
    }

    private void ReadInput()
    {
        hor = Input.GetAxisRaw("Horizontal");
        ver = Input.GetAxisRaw("Vertical");

        input = new Vector2(hor, ver);

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }
    }

    private void Move()
    {
        if (Rb == null)
        {
            return;
        }

        float speedMultiplier = 1f;
        float acceleration = GroundAcceleration;
        float deceleration = GroundDeceleration;

        if (SurfaceInteractor != null && SurfaceInteractor.HasActiveSurface)
        {
            speedMultiplier = SurfaceInteractor.CurrentSpeedMultiplier;
            acceleration = GroundAcceleration * SurfaceInteractor.CurrentAccelerationMultiplier;
            deceleration = GroundDeceleration * SurfaceInteractor.CurrentDecelerationMultiplier;
        }
        else if (WaterEffect != null)
        {
            speedMultiplier = WaterEffect.GetCurrentMoveMultiplier();
            acceleration = WaterEffect.GetCurrentAcceleration(GroundAcceleration);
            deceleration = WaterEffect.GetCurrentDeceleration(GroundDeceleration);
        }

        float finalSpeed = (Speed + AddSpeed) * speedMultiplier;
        targetVelocity = input * finalSpeed;

        float moveRate = HasMovementInput ? acceleration : deceleration;

        Rb.linearVelocity = Vector2.MoveTowards(
            Rb.linearVelocity,
            targetVelocity,
            moveRate * Time.fixedDeltaTime
        );
    }

    private void UpdateFlip()
    {
        if (PlayerSprite == null)
        {
            return;
        }

        if (hor > 0)
        {
            PlayerSprite.flipX = false;
        }
        else if (hor < 0)
        {
            PlayerSprite.flipX = true;
        }
    }

    private void UpdateInvulnerability()
    {
        if (invulnerabilityTimer <= 0f)
        {
            return;
        }

        invulnerabilityTimer -= Time.deltaTime;

        if (invulnerabilityTimer < 0f)
        {
            invulnerabilityTimer = 0f;
        }

        if (ShowInvulnerabilityDebug)
        {
            Debug.Log("Player invulnerable: " + invulnerabilityTimer.ToString("F2"));
        }
    }

    public void StartInvulnerability(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        invulnerabilityTimer = Mathf.Max(invulnerabilityTimer, duration);
    }

    public void SetMovementLocked(bool isLocked)
    {
        movementLocked = isLocked;

        if (movementLocked && Rb != null)
        {
            Rb.linearVelocity = Vector2.zero;
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsInvulnerable)
        {
            return;
        }

        float finalDamage = Mathf.Max(0f, damage - Armor);

        CurrHp -= finalDamage;
        CurrHp = Mathf.Clamp(CurrHp, 0f, CurrentMaxHp);

        HealthCheck();
    }

    private void ShowRewardResult(string RewardText)
    {
        ChestRewardPanel.SetActive(true);
        ChestRewardText.text = RewardText;
    }

    public void GiveArmor(int Add)
    {
        Armor += Add;
        Debug.Log("Passive Add Armor" + Add);
        ShowRewardResult("Armor " + Add);
    }

    public void GiveHPRegen(int Add)
    {
        HealthRegen += Add;
        Debug.Log("Passive Add HpRegen" + Add);
        ShowRewardResult("health regeneration +" + Add);
    }

    public void GiveMaxHp(int Add)
    {
        AddMaxHp += Add;

        CurrHp += Add;
        CurrHp = Mathf.Clamp(CurrHp, 0f, CurrentMaxHp);

        UpdateHealthVisual();
        Debug.Log("Passive Add MaxHp" + Add);
        ShowRewardResult("Max hp +" + Add);
    }

    public void GiveDmg(int Add)
    {
        AddDmg += Add;
        Debug.Log("Passive Add Dmg" + Add);
        ShowRewardResult("Damage +" + Add);
    }

    public void GiveAttackScale(float Add)
    {
        AddAttackScale += Add;
        Debug.Log("Passive Add AttackScale" + Add);
        ShowRewardResult("Радиус скиллов увеличен на " + Add);
    }

    public void GiveSpeed(int Add)
    {
        AddSpeed += Add;
        Debug.Log("Passive Add Speed MoveSpeed" + Add);
        ShowRewardResult("Move speed " + Add);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            float dmg = enemy.GiveDmg();
            TakeDamage(dmg);
        }
    }

    private void HealthCheck()
    {
        UpdateHealthVisual();

        if (CurrHp <= 0f)
        {
            //dead
            SceneManager.LoadScene("MainMenuScene");
        }
    }

    private void UpdateHealthVisual()
    {
        if (HealthVisual != null && CurrentMaxHp > 0f)
        {
            HealthVisual.fillAmount = CurrHp / CurrentMaxHp;
        }
    }

    public void ApplyXP(int NewXP)
    {
        if (LevelMan != null)
        {
            LevelMan.takeXP(NewXP);
        }
    }
}