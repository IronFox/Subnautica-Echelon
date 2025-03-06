using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSimulation : MonoBehaviour
{
    private EchelonControl control;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        control = GetComponent<EchelonControl>();
        rb = GetComponent<Rigidbody>();
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
        control.triggerActive = Input.GetKey(KeyCode.Mouse0);
        control.triggerWasActivated = Input.GetKeyDown(KeyCode.Mouse0);
        
    }

    void FixedUpdate()
    {
        if (control.isBoarded && !control.outOfWater && !control.isDocked)
        {
            if (rb == null)
            {
                return;
            }
            var forwardAccel = control.forwardAxis * 800;

            try
            {
                rb.AddRelativeForce(0, 0, forwardAccel, ForceMode.Acceleration);
            }
            catch (Exception ex)
            {
                ConsoleControl.WriteException("FixedUpdate()", ex);
            }


        }
        if (control.outOfWater)
        {
            rb.drag = 0.1f;
            rb.useGravity = true;
        }
        else
        {
            rb.drag = 10f;
            rb.useGravity = false;
        }
    }
}
