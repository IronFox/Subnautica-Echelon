using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSimulation : MonoBehaviour
{
    private SubControl control;
    // Start is called before the first frame update
    void Start()
    {
        control = GetComponent<SubControl>();
    }

    // Update is called once per frame
    void Update()
    {
        control.forwardAxis = Input.GetAxis("Vertical");
        control.rightAxis = Input.GetAxis("Horizontal");
        control.upAxis = Input.GetAxis("Jump") - (Input.GetKey(KeyCode.LeftControl) ? 1 : 0);
        control.overdriveActive = Input.GetKey(KeyCode.LeftShift);
        control.freeCamera = Input.GetMouseButton(1);
        control.zoomAxis = -Input.GetAxis("Mouse ScrollWheel");
    }
}
