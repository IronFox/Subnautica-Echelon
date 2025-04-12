using UnityEngine;

public class RailgunSprite : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var q = transform.rotation;
        q.SetLookRotation(CameraUtil.GetTransform(nameof(RailgunSprite)).position - transform.position);
        transform.rotation = q;
    }
}
