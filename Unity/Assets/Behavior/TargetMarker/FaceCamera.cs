using System.Collections;
using System.Collections.Generic;
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
        //Debug.Log(Camera.main.transform.right);
        var camera = CameraUtil.GetTransform(nameof(FaceCamera));
        if (camera != null)
            transform.rotation = camera.rotation;
    }
}
