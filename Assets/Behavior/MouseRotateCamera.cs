using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseRotateCamera : MonoBehaviour
{
    public float lookSpeed = 3;
    private Vector2 rotation = Vector2.zero;

    private RotateCamera dtc;
    // Start is called before the first frame update
    void Start()
    {
        dtc = GetComponent<RotateCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        dtc.rotationAxisX = Input.GetAxis("Mouse X");
        dtc.rotationAxisY = Input.GetAxis("Mouse Y");

    }
}
