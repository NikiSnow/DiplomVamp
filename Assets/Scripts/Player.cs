using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Rigidbody2D Rb;
    [SerializeField] private SpriteRenderer PlayerSprite;
    [SerializeField] private Collider2D MainCollider;
    [SerializeField] private PlayerWaterEffect WaterEffect;
    [SerializeField] private SurfaceInteractor SurfaceInteractor;

    [Header("Movement")]
    [SerializeField] private float Speed = 5f;
    [SerializeField] private float GroundAcceleration = 22f;
    [SerializeField] private float GroundDeceleration = 28f;

    [Header("Health")]
    [SerializeField] private float MaxHp = 100f;
    [SerializeField] private float CurrHp = 100f;

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

    public SurfaceInteractor CurrentSurfaceInteractor => SurfaceInteractor;

    private void Reset()
    {
        Rb = GetComponent<Rigidbody2D>();
        MainCollider = GetComponent<Collider2D>();
        PlayerSprite = GetComponentInChildren<SpriteRenderer>();
        WaterEffect = GetComponent<PlayerWaterEffect>();
        SurfaceInteractor = GetComponent<SurfaceInteractor>();
    }

    private void Awake()
    {
        if (Rb == null)
        {
            Rb = GetComponent<Rigidbody2D>();
        }

        if (MainCollider == null)
        {
            MainCollider = GetComponent<Collider2D>();
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

        CurrHp = Mathf.Clamp(CurrHp, 0f, MaxHp);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            float dmg = enemy.GiveDmg();
            TakeDamage(dmg);
        }

        XPBlob xp = collision.gameObject.GetComponent<XPBlob>();

        if (xp != null)
        {
            // xp.takeXP();
        }
    }

    private void HealthCheck()
    {
        if (CurrHp <= 0)
        {
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}