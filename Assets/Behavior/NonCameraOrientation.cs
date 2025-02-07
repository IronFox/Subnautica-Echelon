using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonCameraOrientation : MonoBehaviour
{
    public float rightRotationSpeed;
    public float upRotationSpeed;
    public bool isActive;
    public Transform target;
    private float rotX, rotY;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
        {
            //transform.rotation = target.rotation;
            rotX = target.eulerAngles.x;
            rotY = target.eulerAngles.y;
            if (rotX > 180)
                rotX -= 380;
            Debug.Log(rotX);
        }
        else
        {
            rotY += rightRotationSpeed * Time.deltaTime;
            rotX += upRotationSpeed * Time.deltaTime;
            rotX = Mathf.Clamp(rotX, -88f, 88f);
            transform.eulerAngles = new Vector3(rotX, rotY, 0);
        }
    }
}
