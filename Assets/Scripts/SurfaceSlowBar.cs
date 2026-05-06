using UnityEngine;

public class SurfaceSlowBar : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Player Player;
    [SerializeField] private SurfaceInteractor SurfaceInteractor;

    [Header("Position")]
    [SerializeField] private Vector3 LocalOffset = new Vector3(0f, 1.15f, 0f);

    [Header("Size")]
    [SerializeField] private float Width = 0.9f;
    [SerializeField] private float Height = 0.08f;

    [Header("Fill Logic")]
    [SerializeField] private float MaxPenaltyForFullBar = 0.6f;
    [SerializeField] private float SmoothSpeed = 10f;
    [SerializeField] private bool HideWhenNormal = true;

    [Header("Colors")]
    [SerializeField] private Color BackgroundColor = new Color(0f, 0f, 0f, 0.65f);
    [SerializeField] private Color DefaultFillColor = new Color(1f, 1f, 1f, 0.95f);
    [SerializeField] private Color WaterFillColor = new Color(0.2f, 0.7f, 1f, 0.95f);
    [SerializeField] private Color SandFillColor = new Color(1f, 0.85f, 0.25f, 0.95f);
    [SerializeField] private Color MudFillColor = new Color(0.45f, 0.25f, 0.1f, 0.95f);
    [SerializeField] private Color IceFillColor = new Color(0.7f, 1f, 1f, 0.95f);

    [Header("Sorting")]
    [SerializeField] private string SortingLayerName = "Default";
    [SerializeField] private int SortingOrder = 100;

    private Transform barRoot;
    private Transform fillTransform;

    private SpriteRenderer backgroundRenderer;
    private SpriteRenderer fillRenderer;

    private float currentFill = 0f;

    private static Sprite pixelSprite;

    private void Reset()
    {
        Player = GetComponent<Player>();
        SurfaceInteractor = GetComponent<SurfaceInteractor>();
    }

    private void Awake()
    {
        if (Player == null)
        {
            Player = GetComponent<Player>();
        }

        if (SurfaceInteractor == null)
        {
            SurfaceInteractor = GetComponent<SurfaceInteractor>();
        }

        CreateBar();
    }

    private void Update()
    {
        UpdateBar();
    }

    private void CreateBar()
    {
        Sprite sprite = GetPixelSprite();

        GameObject rootObject = new GameObject("Surface Slow Bar");
        rootObject.transform.SetParent(transform);
        rootObject.transform.localPosition = LocalOffset;
        rootObject.transform.localRotation = Quaternion.identity;
        rootObject.transform.localScale = Vector3.one;

        barRoot = rootObject.transform;

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(barRoot);
        backgroundObject.transform.localPosition = Vector3.zero;
        backgroundObject.transform.localRotation = Quaternion.identity;
        backgroundObject.transform.localScale = new Vector3(Width, Height, 1f);

        backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = sprite;
        backgroundRenderer.color = BackgroundColor;
        backgroundRenderer.sortingLayerName = SortingLayerName;
        backgroundRenderer.sortingOrder = SortingOrder;

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(barRoot);
        fillObject.transform.localPosition = Vector3.zero;
        fillObject.transform.localRotation = Quaternion.identity;
        fillObject.transform.localScale = new Vector3(0f, Height * 0.72f, 1f);

        fillTransform = fillObject.transform;

        fillRenderer = fillObject.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = sprite;
        fillRenderer.color = DefaultFillColor;
        fillRenderer.sortingLayerName = SortingLayerName;
        fillRenderer.sortingOrder = SortingOrder + 1;

        if (HideWhenNormal)
        {
            barRoot.gameObject.SetActive(false);
        }
    }

    private void UpdateBar()
    {
        if (barRoot == null || fillTransform == null)
        {
            return;
        }

        barRoot.localPosition = LocalOffset;

        float speedMultiplier = 1f;

        if (Player != null)
        {
            speedMultiplier = Player.CurrentSpeedMultiplier;
        }

        float targetFill = 0f;

        if (speedMultiplier < 0.99f)
        {
            float penalty = 1f - speedMultiplier;
            targetFill = Mathf.Clamp01(penalty / MaxPenaltyForFullBar);
        }

        currentFill = Mathf.MoveTowards(
            currentFill,
            targetFill,
            SmoothSpeed * Time.deltaTime
        );

        bool shouldShow = !HideWhenNormal || currentFill > 0.01f;

        if (barRoot.gameObject.activeSelf != shouldShow)
        {
            barRoot.gameObject.SetActive(shouldShow);
        }

        if (!shouldShow)
        {
            return;
        }

        float fillWidth = Width * currentFill;

        fillTransform.localScale = new Vector3(fillWidth, Height * 0.72f, 1f);
        fillTransform.localPosition = new Vector3(
            -Width * 0.5f + fillWidth * 0.5f,
            0f,
            -0.01f
        );

        if (fillRenderer != null)
        {
            fillRenderer.color = GetSurfaceColor();
        }
    }

    private Color GetSurfaceColor()
    {
        if (SurfaceInteractor == null || !SurfaceInteractor.HasActiveSurface)
        {
            return DefaultFillColor;
        }

        switch (SurfaceInteractor.CurrentSurfaceType)
        {
            case PhysicsSurfaceType.Water:
                return WaterFillColor;

            case PhysicsSurfaceType.Sand:
                return SandFillColor;

            case PhysicsSurfaceType.Mud:
                return MudFillColor;

            case PhysicsSurfaceType.Ice:
                return IceFillColor;

            default:
                return DefaultFillColor;
        }
    }

    private Sprite GetPixelSprite()
    {
        if (pixelSprite != null)
        {
            return pixelSprite;
        }

        Texture2D texture = new Texture2D(1, 1);
        texture.name = "Runtime Pixel Sprite";
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        pixelSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f
        );

        return pixelSprite;
    }
}