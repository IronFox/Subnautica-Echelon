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

    private LookForward look;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        look = GetComponent<LookForward>();
    }

    // Update is called once per frame
    void Update()
    {
        look.enabled = !freeCamera;
        forwardLeft.thrust = forwardAxis + look.HorizontalRotationIntent*0.001f;
        forwardRight.thrust = forwardAxis - look.HorizontalRotationIntent*0.001f;
        backLeft.thrust = -forwardLeft.thrust;
        backRight.thrust = -forwardRight.thrust;
    }

    void FixedUpdate()
    {
        rb.AddRelativeForce(0, 0, forwardAxis * (regularForwardAcceleration + (overdriveActive && forwardAxis > 0 ? overdriveForwardAcceleration : 0)));
        rb.AddRelativeForce(rightAxis * strafeAcceleration, 0,0);
        rb.AddRelativeForce(0, upAxis * strafeAcceleration, 0);
    }
}
