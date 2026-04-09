using UnityEngine;

public class XPBlob : MonoBehaviour
{
    [SerializeField] int XPAmount = 10;

    void Update()
    {

    }

    public int takeXP()
    {
        Destroy(this.gameObject);
        return XPAmount;
    }
}
