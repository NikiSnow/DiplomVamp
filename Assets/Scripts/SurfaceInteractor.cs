using System.Collections.Generic;
using UnityEngine;

public class SurfaceInteractor : MonoBehaviour
{
    private static readonly List<SurfaceInteractor> allInteractors = new List<SurfaceInteractor>();

    public static IReadOnlyList<SurfaceInteractor> AllInteractors => allInteractors;

    [Header("Target")]
    [SerializeField] private SurfaceTargetType targetType = SurfaceTargetType.Player;

    [Header("Visual Links")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private readonly List<PhysicsSurfaceZone> activeZones = new List<PhysicsSurfaceZone>();

    private PhysicsSurfaceZone currentZone;
    private Color defaultColor;
    private float stayEffectTimer;

    public SurfaceTargetType TargetType => targetType;
    public bool HasActiveSurface => currentZone != null;
    public PhysicsSurfaceZone CurrentZone => currentZone;

    public string CurrentSurfaceName
    {
        get
        {
            if (currentZone == null)
            {
                return "Normal";
            }

            return currentZone.SurfaceName;
        }
    }

    public PhysicsSurfaceType CurrentSurfaceType
    {
        get
        {
            if (currentZone == null)
            {
                return PhysicsSurfaceType.Normal;
            }

            return currentZone.SurfaceType;
        }
    }

    public float CurrentSpeedMultiplier
    {
        get
        {
            if (currentZone == null)
            {
                return 1f;
            }

            return currentZone.SpeedMultiplier;
        }
    }

    public float CurrentAccelerationMultiplier
    {
        get
        {
            if (currentZone == null)
            {
                return 1f;
            }

            return currentZone.AccelerationMultiplier;
        }
    }

    public float CurrentDecelerationMultiplier
    {
        get
        {
            if (currentZone == null)
            {
                return 1f;
            }

            return currentZone.DecelerationMultiplier;
        }
    }

    public float CurrentDashMultiplier
    {
        get
        {
            if (currentZone == null)
            {
                return 1f;
            }

            return currentZone.DashMultiplier;
        }
    }

    public float CurrentSlowAmount
    {
        get
        {
            if (currentZone == null)
            {
                return 0f;
            }

            return Mathf.Clamp01(1f - currentZone.SpeedMultiplier);
        }
    }

    private void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (!allInteractors.Contains(this))
        {
            allInteractors.Add(this);
        }
    }

    private void OnDisable()
    {
        if (allInteractors.Contains(this))
        {
            allInteractors.Remove(this);
        }
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        UpdateStayEffect();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PhysicsSurfaceZone zone = GetZoneFromCollider(other);

        if (zone == null)
        {
            return;
        }

        if (!zone.Affects(targetType))
        {
            return;
        }

        if (!activeZones.Contains(zone))
        {
            activeZones.Add(zone);
        }

        SpawnEnterEffect(zone);
        RecalculateCurrentZone();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PhysicsSurfaceZone zone = GetZoneFromCollider(other);

        if (zone == null)
        {
            return;
        }

        if (activeZones.Contains(zone))
        {
            activeZones.Remove(zone);
        }

        RecalculateCurrentZone();
    }

    private PhysicsSurfaceZone GetZoneFromCollider(Collider2D other)
    {
        PhysicsSurfaceZone zone = other.GetComponent<PhysicsSurfaceZone>();

        if (zone == null)
        {
            zone = other.GetComponentInParent<PhysicsSurfaceZone>();
        }

        return zone;
    }

    private void RecalculateCurrentZone()
    {
        activeZones.RemoveAll(zone => zone == null);

        PhysicsSurfaceZone previousZone = currentZone;
        currentZone = GetBestZone();

        ApplyVisualState();

        if (enableDebugLogs && previousZone != currentZone)
        {
            Debug.Log(
                "[Surface] " +
                gameObject.name +
                " surface: " +
                CurrentSurfaceName +
                " | speed x" +
                CurrentSpeedMultiplier.ToString("F2")
            );
        }
    }

    private PhysicsSurfaceZone GetBestZone()
    {
        if (activeZones.Count == 0)
        {
            return null;
        }

        PhysicsSurfaceZone bestZone = activeZones[0];

        for (int i = 1; i < activeZones.Count; i++)
        {
            PhysicsSurfaceZone candidate = activeZones[i];

            if (candidate == null)
            {
                continue;
            }

            if (candidate.Priority > bestZone.Priority)
            {
                bestZone = candidate;
            }
            else if (candidate.Priority == bestZone.Priority &&
                     candidate.SpeedMultiplier < bestZone.SpeedMultiplier)
            {
                bestZone = candidate;
            }
        }

        return bestZone;
    }

    private void ApplyVisualState()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (currentZone != null && currentZone.ApplyTint)
        {
            spriteRenderer.color = currentZone.TintColor;
        }
        else
        {
            spriteRenderer.color = defaultColor;
        }
    }

    private void SpawnEnterEffect(PhysicsSurfaceZone zone)
    {
        if (zone == null)
        {
            return;
        }

        if (zone.EnterEffectPrefab == null)
        {
            return;
        }

        Instantiate(zone.EnterEffectPrefab, transform.position, Quaternion.identity);
    }

    private void UpdateStayEffect()
    {
        if (currentZone == null)
        {
            stayEffectTimer = 0f;
            return;
        }

        if (currentZone.StayEffectPrefab == null)
        {
            return;
        }

        stayEffectTimer += Time.deltaTime;

        if (stayEffectTimer >= currentZone.StayEffectInterval)
        {
            Instantiate(currentZone.StayEffectPrefab, transform.position, Quaternion.identity);
            stayEffectTimer = 0f;
        }
    }
}