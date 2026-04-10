using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Rigidbody2D Rb;
    [SerializeField] private SpriteRenderer PlayerSprite;
    [SerializeField] private PlayerWaterEffect WaterEffect;

    [Header("Movement")]
    [SerializeField] private float Speed = 5f;
    [SerializeField] private float GroundAcceleration = 22f;
    [SerializeField] private float GroundDeceleration = 28f;

    [Header("Health")]
    [SerializeField] private float MaxHp = 100f;
    [SerializeField] private float CurrHp = 100f;

    [SerializeField] LevelManager LevelMan;

    [SerializeField] Image HealthVisual;

    private float hor = 0f;
    private float ver = 0f;

    private Vector2 input;
    private Vector2 targetVelocity;

    public Vector2 InputVector => input;
    public bool HasMovementInput => input.sqrMagnitude > 0.0001f;
    public Rigidbody2D PlayerRb => Rb;
    public float BaseSpeed => Speed;

    private void Reset()
    {

    }

    private void Awake()
    {
        CurrHp = Mathf.Clamp(CurrHp, 0f, MaxHp);
    }

    private void Update()
    {
        ReadInput();
        UpdateFlip();
    }

    private void FixedUpdate()
    {
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

        if (WaterEffect != null)
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
            CurrHp -= dmg;
            HealthCheck();
            //Debug.Log("Enemy");
            return;
        }
    }

    private void HealthCheck()
    {

        HealthVisual.fillAmount = CurrHp / MaxHp;
        if (CurrHp <= 0)
        {
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }

    public void ApplyXP(int NewXP)
    {
        LevelMan.takeXP(NewXP);
    }
}