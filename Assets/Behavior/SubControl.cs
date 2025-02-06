using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubControl : MonoBehaviour
{
    public float forwardAxis;
    public float rightAxis;
    public float upAxis;

    public bool overdriveActive;
    public bool outOfWater;
    public bool freeCamera;

    public float regularForwardAcceleration = 100000;
    public float overdriveForwardAcceleration = 200000;
    public float strafeAcceleration = 50000;

    public DriveControl forwardLeft;
    public DriveControl backLeft;
    public DriveControl forwardRight;
    public DriveControl backRight;

    public NonCameraOrientation nonCameraOrientation;
    public Transform cameraOrientation;

    private PointNoseInDirection look;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        look = GetComponent<PointNoseInDirection>();
    }

    // Update is called once per frame
    void Update()
    {
        look.targetOrientation = freeCamera ? nonCameraOrientation.transform : cameraOrientation;

        nonCameraOrientation.rightRotationSpeed = rightAxis * 100;
        nonCameraOrientation.upRotationSpeed = -upAxis * 100;
        nonCameraOrientation.isActive = freeCamera;

        forwardLeft.thrust = forwardAxis + look.HorizontalRotationIntent*0.001f;
        forwardRight.thrust = forwardAxis - look.HorizontalRotationIntent*0.001f;
        backLeft.thrust = -forwardLeft.thrust;
        backRight.thrust = -forwardRight.thrust;

    }

    void FixedUpdate()
    {
        
        rb.AddRelativeForce(0, 0, forwardAxis * (regularForwardAcceleration + (overdriveActive && forwardAxis > 0 ? overdriveForwardAcceleration : 0)));
        if (!freeCamera)
        {
            //var rAxis = M.FlatNormalized(transform.right);
            rb.AddForce(look.targetOrientation.right * rightAxis * strafeAcceleration);
            rb.AddForce(look.targetOrientation.up * upAxis * strafeAcceleration);
        }
    }
}
