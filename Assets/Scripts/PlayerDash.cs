using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Player Player;
    [SerializeField] private Rigidbody2D Rb;
    [SerializeField] private SurfaceInteractor SurfaceInteractor;

    [Header("Input")]
    [SerializeField] private KeyCode DashKey = KeyCode.Space;

    [Header("Dash Movement")]
    [SerializeField] private float DashSpeed = 16f;
    [SerializeField] private float DashDuration = 0.16f;
    [SerializeField] private float DashCooldown = 1.1f;
    [SerializeField] private float EndDashVelocityMultiplier = 0.25f;

    [Header("Dash Buff")]
    [SerializeField] private float DashPowerMultiplier = 1.25f;
    [SerializeField] private float DashDurationBonus = 0.04f;
    [SerializeField] private float DashCooldownReduction = 0.15f;

    [Header("Invulnerability")]
    [SerializeField] private float PostDashInvulnerabilityTime = 0.35f;

    [Header("Enemy Push")]
    [SerializeField] private LayerMask EnemyLayer = ~0;
    [SerializeField] private float PushRadius = 1.25f;
    [SerializeField] private float PushForce = 9f;
    [SerializeField] private float EnemyControlLockTime = 0.22f;
    [SerializeField] private float DashDamage = 0f;

    [Header("Enemy Push Buff")]
    [SerializeField] private float PushPowerMultiplier = 1.35f;
    [SerializeField] private float PushRadiusBonus = 0.2f;
    [SerializeField] private float EnemyControlLockBonus = 0.08f;

    [Header("Effects")]
    [SerializeField] private GameObject DashStartEffectPrefab;
    [SerializeField] private GameObject DashHitEffectPrefab;
    [SerializeField] private TrailRenderer DashTrail;
    [SerializeField] private ParticleSystem DashParticles;

    [Header("Debug")]
    [SerializeField] private bool DrawDebugRadius = true;

    private readonly HashSet<Enemy> hitEnemiesThisDash = new HashSet<Enemy>();

    private Vector2 lastMoveDirection = Vector2.right;
    private Vector2 dashDirection = Vector2.right;

    private float dashTimer = 0f;
    private float cooldownTimer = 0f;

    private int lastDashPushCount = 0;

    public bool IsDashing => dashTimer > 0f;
    public int LastDashPushCount => lastDashPushCount;
    public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
    public bool IsReady => cooldownTimer <= 0f && !IsDashing;

    [SerializeField] Animator Anim;

    private void Reset()
    {
        Player = GetComponent<Player>();
        Rb = GetComponent<Rigidbody2D>();
        SurfaceInteractor = GetComponent<SurfaceInteractor>();
        DashTrail = GetComponentInChildren<TrailRenderer>();
        DashParticles = GetComponentInChildren<ParticleSystem>();
    }

    private void Awake()
    {
        if (Player == null)
        {
            Player = GetComponent<Player>();
        }

        if (Rb == null)
        {
            Rb = GetComponent<Rigidbody2D>();
        }

        if (SurfaceInteractor == null)
        {
            SurfaceInteractor = GetComponent<SurfaceInteractor>();
        }

        if (DashTrail != null)
        {
            DashTrail.emitting = false;
        }

        if (DashParticles != null)
        {
            DashParticles.Stop();
        }
    }

    private void Update()
    {
        UpdateCooldown();
        UpdateLastMoveDirection();
        TryStartDash();
    }

    private void FixedUpdate()
    {
        if (!IsDashing)
        {
            return;
        }

        UpdateDashMovement();
        PushEnemiesDuringDash();
    }

    private void UpdateCooldown()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void UpdateLastMoveDirection()
    {
        if (Player == null)
        {
            return;
        }

        if (Player.HasMovementInput)
        {
            lastMoveDirection = Player.InputVector.normalized;
        }
    }

    private void TryStartDash()
    {
        if (!Input.GetKeyDown(DashKey))
        {
            return;
        }

        if (!IsReady)
        {
            return;
        }

        StartDash();
    }

    private void StartDash()
    {
        Anim.SetTrigger("Dash");
        dashDirection = lastMoveDirection;

        if (dashDirection.sqrMagnitude <= 0.0001f)
        {
            dashDirection = Vector2.right;
        }

        dashDirection.Normalize();

        float finalDashDuration = GetFinalDashDuration();
        float finalCooldown = GetFinalDashCooldown();

        dashTimer = finalDashDuration;
        cooldownTimer = finalCooldown;

        lastDashPushCount = 0;
        hitEnemiesThisDash.Clear();

        if (Player != null)
        {
            Player.SetMovementLocked(true);
            Player.StartInvulnerability(finalDashDuration + PostDashInvulnerabilityTime);
        }

        if (DashTrail != null)
        {
            DashTrail.Clear();
            DashTrail.emitting = true;
        }

        if (DashParticles != null)
        {
            DashParticles.Play();
        }

        if (DashStartEffectPrefab != null)
        {
            Instantiate(DashStartEffectPrefab, transform.position, Quaternion.identity);
        }

        PushEnemiesDuringDash();
    }

    private void UpdateDashMovement()
    {
        dashTimer -= Time.fixedDeltaTime;

        float surfaceDashMultiplier = 1f;

        if (SurfaceInteractor != null && SurfaceInteractor.HasActiveSurface)
        {
            surfaceDashMultiplier = SurfaceInteractor.CurrentDashMultiplier;
        }

        if (Rb != null)
        {
            Rb.linearVelocity = dashDirection * GetFinalDashSpeed() * surfaceDashMultiplier;
        }

        if (dashTimer <= 0f)
        {
            EndDash();
        }
    }

    private void EndDash()
    {
        dashTimer = 0f;

        if (Player != null)
        {
            Player.SetMovementLocked(false);
        }

        if (Rb != null)
        {
            Rb.linearVelocity *= EndDashVelocityMultiplier;
        }

        if (DashTrail != null)
        {
            DashTrail.emitting = false;
        }

        if (DashParticles != null)
        {
            DashParticles.Stop();
        }
    }

    private void PushEnemiesDuringDash()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            GetFinalPushRadius(),
            EnemyLayer
        );

        float playerSurfacePushMultiplier = 1f;

        if (SurfaceInteractor != null && SurfaceInteractor.HasActiveSurface)
        {
            playerSurfacePushMultiplier = SurfaceInteractor.CurrentDashMultiplier;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
            {
                continue;
            }

            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy == null)
            {
                enemy = hit.GetComponentInParent<Enemy>();
            }

            if (enemy == null)
            {
                continue;
            }

            if (hitEnemiesThisDash.Contains(enemy))
            {
                continue;
            }

            Vector2 pushDirection = enemy.transform.position - transform.position;

            if (pushDirection.sqrMagnitude <= 0.0001f)
            {
                pushDirection = dashDirection;
            }

            pushDirection.Normalize();

            enemy.ApplyKnockback(
                pushDirection,
                GetFinalPushForce() * playerSurfacePushMultiplier,
                GetFinalEnemyControlLockTime(),
                DashDamage
            );

            hitEnemiesThisDash.Add(enemy);
            lastDashPushCount++;

            if (DashHitEffectPrefab != null)
            {
                Instantiate(
                    DashHitEffectPrefab,
                    enemy.transform.position,
                    Quaternion.identity
                );
            }
        }
    }

    private float GetFinalDashSpeed()
    {
        return DashSpeed * DashPowerMultiplier;
    }

    private float GetFinalDashDuration()
    {
        return Mathf.Max(0.01f, DashDuration + DashDurationBonus);
    }

    private float GetFinalDashCooldown()
    {
        return Mathf.Max(0.1f, DashCooldown - DashCooldownReduction);
    }

    private float GetFinalPushForce()
    {
        return PushForce * PushPowerMultiplier;
    }

    private float GetFinalPushRadius()
    {
        return Mathf.Max(0.05f, PushRadius + PushRadiusBonus);
    }

    private float GetFinalEnemyControlLockTime()
    {
        return Mathf.Max(0f, EnemyControlLockTime + EnemyControlLockBonus);
    }

    private void OnDrawGizmosSelected()
    {
        if (!DrawDebugRadius)
        {
            return;
        }

        Gizmos.DrawWireSphere(transform.position, GetFinalPushRadius());
    }
}