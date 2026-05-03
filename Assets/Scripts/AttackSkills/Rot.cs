using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Rot : MonoBehaviour
{
    [SerializeField] Transform Player;

    private void Update()
    {
        this.gameObject.transform.position = new Vector3(Player.position.x, Player.position.y, Player.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.InRot = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.InRot = false;
        }
    }
}
