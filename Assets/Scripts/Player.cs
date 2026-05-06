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
    [SerializeField] int Armor = 0; //White
    [SerializeField] int HealthRegen = 0; //White 
    [SerializeField] int AddMaxHp = 0; //Green
    [SerializeField] int AddDmg = 0; //Green
    [SerializeField] float AddAttackScale = 1; //Blue
    [SerializeField] int AddSpeed = 0; //Blue

    private float hor = 0f;
    private float ver = 0f;

    private Vector2 input;
    private Vector2 targetVelocity;

    private bool movementLocked = false;

    public Vector2 InputVector => input;
    public bool HasMovementInput => input.sqrMagnitude > 0.0001f;
    public Rigidbody2D PlayerRb => Rb;
    public float BaseSpeed => Speed;

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

        CurrHp = Mathf.Clamp(CurrHp, 0f, MaxHp);
        UpdateHealthVisual();
    }

    public void GiveArmor(int Add)
    {
        Armor = Armor + Add;
    }
    public void GiveHPRegen(int Add)
    {
        HealthRegen = HealthRegen + Add;
    }
    public void GiveMaxHp(int Add)
    {
        AddMaxHp = AddMaxHp + Add;
    }
    public void GiveDmg(int Add)
    {
        AddDmg = AddDmg + Add;
    }
    public void GiveAttackScale(float Add)
    {
        AddAttackScale = AddAttackScale + Add;
    }
    public void GiveSpeed(int Add)
    {
        AddSpeed = AddSpeed + Add;
    }

    private void Update()
    {
        ReadInput();
        UpdateFlip();
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

        float finalSpeed = Speed * speedMultiplier;
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
            PlayerSprite.flipX = true;
        }
        else if (hor < 0)
        {
            PlayerSprite.flipX = false;
        }
    }

    public void SetMovementLocked(bool isLocked)
    {
        movementLocked = isLocked;
    }

    public void TakeDamage(float damage)
    {
        CurrHp -= damage;
        HealthCheck();
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

        if (CurrHp <= 0)
        {
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }

    private void UpdateHealthVisual()
    {
        if (HealthVisual != null)
        {
            HealthVisual.fillAmount = CurrHp / MaxHp;
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