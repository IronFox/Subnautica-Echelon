using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var camera = CameraUtil.GetTransform(nameof(FaceCamera));
        if (camera != null)
            transform.rotation = camera.rotation;
    }
}
