using UnityEngine;

public class AutoDestroyAfterTime : MonoBehaviour
{
    [SerializeField] private float LifeTime = 1.5f;

    private void Start()
    {
        Destroy(gameObject, LifeTime);
    }
}