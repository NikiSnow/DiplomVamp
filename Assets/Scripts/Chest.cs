using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] public Casino CasinoScr;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CasinoScr.StartGambling();
            Destroy(this.gameObject);
        }
    }
}
