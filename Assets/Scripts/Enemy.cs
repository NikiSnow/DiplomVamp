using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float Speed;
    [SerializeField] Rigidbody2D Rb;
    [SerializeField] float Heath = 25;
    [SerializeField] float Dmg = 1;

    public float GiveDmg()
    {
        return Dmg;
    }
    void TakeDmg()
    {

    }

    void Die()
    {

    }
}
