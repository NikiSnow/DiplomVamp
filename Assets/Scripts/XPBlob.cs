using UnityEngine;

public class XPBlob : MonoBehaviour
{
    [SerializeField] int XPAmount = 10;

    void Update()
    {

    }

    public int takeXP()
    {
        //Debug.Log("XP XP XP");
        Destroy(this.gameObject);
        return XPAmount;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            player.ApplyXP(XPAmount);
            Destroy(this.gameObject);
        }
    }
}
