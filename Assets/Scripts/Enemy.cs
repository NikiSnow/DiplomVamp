using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Enemy : MonoBehaviour
{
    [SerializeField] float Speed;
    [SerializeField] Rigidbody2D Rb;
    [SerializeField] float Heath = 25;
    [SerializeField] float Dmg = 1;

    [SerializeField] Transform target;
    [SerializeField] SpriteRenderer Sprite;
    [SerializeField] Rigidbody2D rb;

    [SerializeField] bool isDispawnable = false;
    [SerializeField] float dispawnDistance;

    [SerializeField] GameObject XPPrefab;

    public bool InRot = false;

    private void Update()
    {
        Vector2 direction = new Vector2(
            target.position.x - transform.position.x,
            target.position.y - transform.position.y);

        float distance = direction.magnitude;
        if (isDispawnable && distance > dispawnDistance)
        {
            Destroy(gameObject);
        }

        // 2. Нормализуем (делаем длину = 1)
        direction.Normalize();

        // 3. Устанавливаем скорость через RigidBody2D
        rb.linearVelocity = new Vector2(
            direction.x * Speed,
            direction.y * Speed
        );

        // Поворот спрайта в зависимости от направления
        if (direction.x > 0)
        {
            Sprite.flipX = true;
        }
        else if (direction.x < 0)
        {
            Sprite.flipX = false;
        }
    }


    private void FixedUpdate() //50 in sec
    {
        if (InRot)
        {
            TakeDmg(0.2f);
        }

        DeathCheck();
    }

    void DeathCheck()
    {
        if (Heath <= 0)
        {
            GameObject Prefab = Instantiate(XPPrefab,transform.position,Quaternion.identity);
            Destroy(this.gameObject);
        }
    }

    public float GiveDmg()
    {
        return Dmg;
    }

    void TakeDmg(float Damage)
    {
        Heath = Heath - Damage;
    }

    void Die()
    {

    }
}
