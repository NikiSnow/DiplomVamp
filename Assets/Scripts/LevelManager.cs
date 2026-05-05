using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] float LevelNeed;
    [SerializeField] float CurrXPAmout;

    [SerializeField] Image XPBar;

    [SerializeField] GameObject Panel;


    private void Start()
    {
        SetVisual();
    }
    public void takeXP(int NewXP)
    {
        CurrXPAmout = CurrXPAmout + NewXP;

        SetVisual();
    }

    void SetVisual()
    {
        //Debug.Log(CurrXPAmout / LevelNeed);
        if (CurrXPAmout >= LevelNeed)
        {
            CurrXPAmout = 0;
            Panel.SetActive(true);
            Time.timeScale = 0;
        }
        XPBar.fillAmount = CurrXPAmout / LevelNeed;
    }

    public void Continue()
    {
        Panel.SetActive(false);
        Time.timeScale = 1;
    }
}
