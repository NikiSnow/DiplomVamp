using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [SerializeField] Rigidbody2D Rb;
    [SerializeField] SpriteRenderer PlayerSprite;

    [SerializeField] float Speed;

    [SerializeField] float MaxHp;
    [SerializeField] float CurrHp;


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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Enemy>())
        {
            float Dmg = collision.GetComponent<Enemy>().GiveDmg();
            CurrHp = CurrHp - Dmg;
            HealthCheck();
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
