using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PhysicsDemoUI : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Player Player;
    [SerializeField] private PlayerDash PlayerDash;
    [SerializeField] private Text InfoText;

    [Header("OnGUI Demo Panel")]
    [SerializeField] private bool UseOnGUI = true;
    [SerializeField] private Vector2 PanelPosition = new Vector2(16f, 16f);
    [SerializeField] private Vector2 PanelSize = new Vector2(370f, 170f);

    [Header("Refresh")]
    [SerializeField] private float RefreshInterval = 0.1f;

    private readonly StringBuilder builder = new StringBuilder();

    private string cachedText = "";
    private float refreshTimer = 0f;

    private void Reset()
    {
        Player = FindObjectOfType<Player>();
        PlayerDash = FindObjectOfType<PlayerDash>();
    }

    private void Awake()
    {
        if (Player == null)
        {
            Player = FindObjectOfType<Player>();
        }

        if (PlayerDash == null)
        {
            PlayerDash = FindObjectOfType<PlayerDash>();
        }
    }

    private void Start()
    {
        UpdateText();
    }

    private void Update()
    {
        refreshTimer += Time.deltaTime;

        if (refreshTimer < RefreshInterval)
        {
            return;
        }

        refreshTimer = 0f;
        UpdateText();
    }

    private void UpdateText()
    {
        builder.Clear();

        string surfaceName = "Normal";
        float speedMultiplier = 1f;
        float dashMultiplier = 1f;

        if (Player != null)
        {
            surfaceName = Player.CurrentSurfaceName;
            speedMultiplier = Player.CurrentSpeedMultiplier;
            dashMultiplier = Player.CurrentDashMultiplier;
        }

        string dashState = "No Dash Script";
        float dashCooldown = 0f;
        int pushedEnemies = 0;

        if (PlayerDash != null)
        {
            if (PlayerDash.IsDashing)
            {
                dashState = "Dashing";
            }
            else if (PlayerDash.IsReady)
            {
                dashState = "Ready";
            }
            else
            {
                dashState = "Cooldown";
            }

            dashCooldown = PlayerDash.CooldownRemaining;
            pushedEnemies = PlayerDash.LastDashPushCount;
        }

        int slowedEnemies = CountSlowedEnemies();

        builder.AppendLine("PHYSICS DEMO");
        builder.AppendLine("----------------------------");
        builder.AppendLine("Surface: " + surfaceName);
        builder.AppendLine("Speed Multiplier: x" + speedMultiplier.ToString("F2"));
        builder.AppendLine("Dash Multiplier: x" + dashMultiplier.ToString("F2"));
        builder.AppendLine("Dash: " + dashState);
        builder.AppendLine("Dash Cooldown: " + dashCooldown.ToString("F1") + " sec");
        builder.AppendLine("Enemies Slowed: " + slowedEnemies);
        builder.AppendLine("Enemies Pushed Last Dash: " + pushedEnemies);

        cachedText = builder.ToString();

        if (InfoText != null)
        {
            InfoText.text = cachedText;
        }
    }

    private int CountSlowedEnemies()
    {
        int count = 0;

        IReadOnlyList<SurfaceInteractor> interactors = SurfaceInteractor.AllInteractors;

        for (int i = 0; i < interactors.Count; i++)
        {
            SurfaceInteractor interactor = interactors[i];

            if (interactor == null)
            {
                continue;
            }

            if (interactor.TargetType != SurfaceTargetType.Enemy)
            {
                continue;
            }

            if (!interactor.HasActiveSurface)
            {
                continue;
            }

            if (interactor.CurrentSpeedMultiplier < 0.99f)
            {
                count++;
            }
        }

        return count;
    }

    private void OnGUI()
    {
        if (!UseOnGUI)
        {
            return;
        }

        Rect rect = new Rect(
            PanelPosition.x,
            PanelPosition.y,
            PanelSize.x,
            PanelSize.y
        );

        GUI.Box(rect, cachedText);
    }
}