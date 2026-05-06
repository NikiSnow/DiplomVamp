using System.Collections;
using UnityEngine;

public class YellowReward : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] Player ThePlayer;
    [SerializeField] float MyCooldown = 1;
    [SerializeField] GameObject MyChild;

    private void OnEnable()
    {
        MyChild.SetActive(true);
    }

    public IEnumerator WaitCD()
    {
        Debug.Log("ChildWaitCD");
        yield return new WaitForSeconds(MyCooldown);
        MyChild.SetActive(true);
        Debug.Log("ChildActive");
    }
}
