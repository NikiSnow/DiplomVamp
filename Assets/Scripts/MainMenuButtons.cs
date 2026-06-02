using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Game";

    [SerializeField] GameObject LoadingPanel;

    public void StartGame()
    {
        LoadingPanel.SetActive(true);
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры");

        Application.Quit();
    }
}
