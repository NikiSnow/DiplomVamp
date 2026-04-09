using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] int LevelNeed;
    [SerializeField] int CurrXPAmout;

    public void takeXP(int NewXP)
    {
        CurrXPAmout = CurrXPAmout + NewXP;
    }
}
