using UnityEngine;

public enum PhysicsSurfaceType
{
    Normal,
    Water,
    Sand,
    Mud,
    Ice
}

public enum SurfaceTargetType
{
    Player,
    Enemy
}

[RequireComponent(typeof(Collider2D))]
public class PhysicsSurfaceZone : MonoBehaviour
{
    [Header("Surface Info")]
    [SerializeField] private string surfaceName = "Water";
    [SerializeField] private PhysicsSurfaceType surfaceType = PhysicsSurfaceType.Water;
    [SerializeField] private int priority = 0;

    [Header("Affected Objects")]
    [SerializeField] private bool affectsPlayer = true;
    [SerializeField] private bool affectsEnemies = true;

    [Header("Movement Physics")]
    [Range(0.1f, 1.5f)]
    [SerializeField] private float speedMultiplier = 0.65f;

    [Range(0.1f, 2f)]
    [SerializeField] private float accelerationMultiplier = 0.75f;

    [Range(0.1f, 2f)]
    [SerializeField] private float decelerationMultiplier = 0.65f;

    [Header("Dash Physics")]
    [Range(0.1f, 1.5f)]
    [SerializeField] private float dashMultiplier = 0.75f;

    [Header("Visual")]
    [SerializeField] private bool applyTint = true;
    [SerializeField] private Color tintColor = Color.white;
    [SerializeField] private GameObject enterEffectPrefab;
    [SerializeField] private GameObject stayEffectPrefab;
    [SerializeField] private float stayEffectInterval = 0.4f;

    public string SurfaceName => surfaceName;
    public PhysicsSurfaceType SurfaceType => surfaceType;
    public int Priority => priority;

    public float SpeedMultiplier => speedMultiplier;
    public float AccelerationMultiplier => accelerationMultiplier;
    public float DecelerationMultiplier => decelerationMultiplier;
    public float DashMultiplier => dashMultiplier;

    public bool ApplyTint => applyTint;
    public Color TintColor => tintColor;

    public GameObject EnterEffectPrefab => enterEffectPrefab;
    public GameObject StayEffectPrefab => stayEffectPrefab;
    public float StayEffectInterval => stayEffectInterval;

    private void Reset()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    public bool Affects(SurfaceTargetType targetType)
    {
        if (targetType == SurfaceTargetType.Player)
        {
            return affectsPlayer;
        }

        if (targetType == SurfaceTargetType.Enemy)
        {
            return affectsEnemies;
        }

        return false;
    }
}