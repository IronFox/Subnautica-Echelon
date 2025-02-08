using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubControl : MonoBehaviour
{
    public float forwardAxis;
    public float rightAxis;
    public float upAxis;
    public float zoomAxis;

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
    public RotateCamera rotateCamera;
    public PositionCamera positionCamera;

    private PointNoseInDirection look;
    private Rigidbody rb;

    private enum CameraState
    {
        IsFree,
        IsBound,
        IsTransitioningToBound
    }
    
    private CameraState state = CameraState.IsBound;

    private void ChangeState(CameraState state)
    {
        Debug.Log($"->{state}");
        this.state = state;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        look = GetComponent<PointNoseInDirection>();
    }

    // Update is called once per frame
    void Update()
    {
        if (freeCamera)
        {
            rotateCamera.AbortTransition();
            ChangeState(CameraState.IsFree);
            look.targetOrientation = nonCameraOrientation.transform;
            nonCameraOrientation.isActive = true;
        }
        else
        {

            switch (state)
            {
                case CameraState.IsTransitioningToBound:
                    if (rotateCamera.IsTransitionDone)
                    {
                        ChangeState(CameraState.IsBound);
                        look.targetOrientation = rotateCamera.transform;
                        nonCameraOrientation.isActive = false;
                        rotateCamera.AbortTransition();
                    }
                    break;
                case CameraState.IsFree:
                    ChangeState(CameraState.IsTransitioningToBound);
                    rotateCamera.BeginTransitionTo(transform);
                    break;

            }
        }


        //look.targetOrientation = freeCamera ? nonCameraOrientation.transform : shipTrailingCamera.transform;

        nonCameraOrientation.rightRotationSpeed = rightAxis * 100;
        nonCameraOrientation.upRotationSpeed = -upAxis * 100;
        

        forwardLeft.thrust = forwardAxis + look.HorizontalRotationIntent*0.001f;
        forwardRight.thrust = forwardAxis - look.HorizontalRotationIntent*0.001f;
        backLeft.thrust = -forwardLeft.thrust;
        backRight.thrust = -forwardRight.thrust;

        positionCamera.zoomAxis = zoomAxis;



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
