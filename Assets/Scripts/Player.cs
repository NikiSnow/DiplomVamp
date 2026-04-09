using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] Rigidbody2D Rb;
    [SerializeField] SpriteRenderer PlayerSprite;

    [SerializeField] float Speed;

    [SerializeField] float MaxHp;
    [SerializeField] float CurrHp;

    [SerializeField] Collider2D MainCollider;

    float hor = 0;
    float ver = 0;

    private void Update()
    {
        Move();
    }

    void Move()
    {
        hor = Input.GetAxis("Horizontal");
        ver = Input.GetAxis("Vertical");
        if (hor != 0 || ver != 0)
        {
            Rb.linearVelocity = new Vector2(Speed * hor, Speed * ver);
        }

        if (hor > 0)
        {
            PlayerSprite.flipX = true;
        }
        else if (hor < 0)
        {
            PlayerSprite.flipX = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("1");
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        // Проверяем, что враг существует И что коллайдер является основным && collision.collider == MainCollider
        if (enemy != null)
        {
            Debug.Log("2");
            float Dmg = enemy.GiveDmg();
            CurrHp = CurrHp - Dmg;
            HealthCheck();
        }

        XPBlob xp = collision.gameObject.GetComponent<XPBlob>();
        if (xp != null)
        {
            //xp.takeXP();
        }
    }

    void HealthCheck()
    {
        if (CurrHp < 0)
        {
            SceneManager.LoadScene(this.gameObject.scene.name);
        }
    }
}
