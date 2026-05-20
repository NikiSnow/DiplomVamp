using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float Speed = 2.5f;
    [SerializeField] private Rigidbody2D Rb;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] public Transform target;
    [SerializeField] private SurfaceInteractor SurfaceInteractor;

    [Header("Stats")]
    [SerializeField] private float Heath = 25f;
    [SerializeField] private float Dmg = 1f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer Sprite;

    [Header("Despawning")]
    [SerializeField] private bool isDispawnable = false;
    [SerializeField] private float dispawnDistance = 35f;

    [Header("Drop")]
    [SerializeField] private GameObject XPPrefab;

    [Header("Knockback Physics")]
    [SerializeField] private float KnockbackFriction = 18f;
    [SerializeField] private float MinKnockbackVelocity = 0.1f;

    [SerializeField] Animator animator;

    public bool InRot = false;

    private float knockbackTimer = 0f;
    private Vector2 knockbackVelocity;

    public bool IsKnockbacked => knockbackTimer > 0f;

    public float CurrentSurfaceSpeedMultiplier
    {
        get
        {
            if (SurfaceInteractor != null)
            {
                return SurfaceInteractor.CurrentSpeedMultiplier;
            }

            return 1f;
        }
    }

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        Rb = rb;
        Sprite = GetComponentInChildren<SpriteRenderer>();
        SurfaceInteractor = GetComponent<SurfaceInteractor>();
    }

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (Rb == null)
        {
            Rb = rb;
        }

        if (rb == null && Rb != null)
        {
            rb = Rb;
        }

        if (Sprite == null)
        {
            Sprite = GetComponentInChildren<SpriteRenderer>();
        }

        if (SurfaceInteractor == null)
        {
            SurfaceInteractor = GetComponent<SurfaceInteractor>();
        }

        TryFindTarget();
    }

    private void Update()
    {
        TryFindTarget();
        UpdateSpriteFlip();
        CheckDespawn();
    }

    private void FixedUpdate()
    {
        HandleRotDamage();

        if (Heath <= 0f)
        {
            DeathCheck();
            return;
        }

        if (HandleKnockback())
        {
            return;
        }

        MoveToTarget();
    }

    private void TryFindTarget()
    {
        if (target != null)
        {
            return;
        }

        Player player = FindObjectOfType<Player>();

        if (player != null)
        {
            target = player.transform;
        }
    }

    private void MoveToTarget()
    {
        if (rb == null || target == null)
        {
            return;
        }

        Vector2 direction = new Vector2(
            target.position.x - transform.position.x,
            target.position.y - transform.position.y
        );

        if (direction.sqrMagnitude <= 0.0001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        direction.Normalize();

        float surfaceSpeedMultiplier = 1f;

        if (SurfaceInteractor != null)
        {
            surfaceSpeedMultiplier = SurfaceInteractor.CurrentSpeedMultiplier;
        }

        rb.linearVelocity = new Vector2(
            direction.x * Speed * surfaceSpeedMultiplier,
            direction.y * Speed * surfaceSpeedMultiplier
        );
    }

    private bool HandleKnockback()
    {
        if (rb == null)
        {
            return false;
        }

        if (knockbackTimer <= 0f)
        {
            return false;
        }

        knockbackTimer -= Time.fixedDeltaTime;

        rb.linearVelocity = knockbackVelocity;

        knockbackVelocity = Vector2.MoveTowards(
            knockbackVelocity,
            Vector2.zero,
            KnockbackFriction * Time.fixedDeltaTime
        );

        if (knockbackVelocity.magnitude <= MinKnockbackVelocity || knockbackTimer <= 0f)
        {
            knockbackTimer = 0f;
            knockbackVelocity = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
        }

        return true;
    }

    public void ApplyKnockback(Vector2 direction, float force, float controlLockTime, float damage)
    {
        if (rb == null)
        {
            return;
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.right;
        }

        direction.Normalize();

        float surfaceKnockbackMultiplier = 1f;

        if (SurfaceInteractor != null && SurfaceInteractor.HasActiveSurface)
        {
            surfaceKnockbackMultiplier = SurfaceInteractor.CurrentDashMultiplier;
        }

        knockbackVelocity = direction * force * surfaceKnockbackMultiplier;
        knockbackTimer = Mathf.Max(knockbackTimer, controlLockTime);

        rb.linearVelocity = knockbackVelocity;

        if (damage > 0f)
        {
            TakeDmg(damage);
        }
    }

    private void HandleRotDamage()
    {
        if (InRot)
        {
            TakeDmg(0.2f);
        }
    }

    private void UpdateSpriteFlip()
    {
        if (Sprite == null || rb == null)
        {
            return;
        }

        if (rb.linearVelocity.x > 0.05f)
        {
            Sprite.flipX = false;
        }
        else if (rb.linearVelocity.x < -0.05f)
        {
            Sprite.flipX = true;
        }

        if (rb.linearVelocityY > 0.05f)
        {
            animator.SetBool("Up", false);
        }
        else
        {
            animator.SetBool("Up", true);
        }
    }

    private void CheckDespawn()
    {
        if (!isDispawnable || target == null)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance > dispawnDistance)
        {
            Destroy(gameObject);
        }
    }

    private void DeathCheck()
    {
        if (Heath > 0f)
        {
            return;
        }

        //if (XPPrefab != null)
        //{
        //    Instantiate(XPPrefab, transform.position, Quaternion.identity);
        //}

        Destroy(gameObject);
    }

    public float GiveDmg()
    {
        return Dmg;
    }

    public void TakeDmg(float Damage)
    {
        Heath -= Damage;

        if (Heath <= 0f)
        {
            DeathCheck();
        }
    }

    private void Die()
    {
        DeathCheck();
    }
}