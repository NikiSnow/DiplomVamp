using System.Collections;
using UnityEngine;

public class YellowReward : MonoBehaviour
{
    [SerializeField] public GameObject Player;
    [SerializeField] public Player ThePlayer;
    [SerializeField] float MyCooldown = 1f;
    [SerializeField] GameObject MyChild;

    private void Start()
    {
        this.transform.localPosition = Vector3.zero;

    }

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
