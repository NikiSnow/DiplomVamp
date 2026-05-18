using System.Collections;
using UnityEngine;

public class YellowReward : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] Player ThePlayer;
    [SerializeField] float MyCooldown = 1f;
    [SerializeField] GameObject MyChild;

    private void OnEnable()
    {
        MyChild.SetActive(true);
    }

    public IEnumerator WaitCD()
    {
        yield return new WaitForSeconds(MyCooldown);
        MyChild.SetActive(true);
    }

    public void StartReload()
    {
        StartCoroutine(WaitCD());
    }
}
