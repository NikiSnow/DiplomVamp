using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] Transform Target;
    [SerializeField] Vector3 Offset;

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position = new Vector3(Target.position.x + Offset.x, Target.position.y + Offset.y, Target.position.z + Offset.z);
    }
}
