using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuButtonsController : MonoBehaviour
{
    [Header("Start Button")]
    [SerializeField] private Button StartButton;
    [SerializeField] private string GameplaySceneName = "GameScene";

    [Header("Exit Button")]
    [SerializeField] private Button ExitButton;

    [Header("Debug")]
    [SerializeField] private bool LogDebug = false;

    private void Awake()
    {
        BindButtons();
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    private void BindButtons()
    {
        if (StartButton != null)
            StartButton.onClick.AddListener(OnStartButtonClicked);

        if (ExitButton != null)
            ExitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void UnbindButtons()
    {
        if (StartButton != null)
            StartButton.onClick.RemoveListener(OnStartButtonClicked);

        if (ExitButton != null)
            ExitButton.onClick.RemoveListener(OnExitButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        if (string.IsNullOrWhiteSpace(GameplaySceneName))
        {
            Debug.LogWarning("[MainMenuButtonsController] Gameplay scene name is empty.");
            return;
        }

        DebugLog($"Loading scene: {GameplaySceneName}");

        SceneManager.LoadScene(GameplaySceneName);
    }

    private void OnExitButtonClicked()
    {
        DebugLog("Exit button clicked.");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void DebugLog(string message)
    {
        if (!LogDebug)
            return;

        Debug.Log($"[MainMenuButtonsController] {message}");
    }
}