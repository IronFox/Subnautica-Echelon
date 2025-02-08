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
    public bool isBoarded;
    public bool positionCameraBelowSub;

    public float regularForwardAcceleration = 100000;
    public float overdriveForwardAcceleration = 200000;
    public float strafeAcceleration = 50000;
    public float rotationDegreesPerSecond = 100;

    public DriveControl forwardLeft;
    public DriveControl backLeft;
    public DriveControl forwardRight;
    public DriveControl backRight;

    public NonCameraOrientation nonCameraOrientation;
    public RotateCamera rotateCamera;
    public PositionCamera positionCamera;
    public Transform cockpitRoot;

    private PointNoseInDirection look;
    private Rigidbody rb;
    private ScreenControl screen;
    private bool currentlyBoarded;

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

    public void Onboard(Transform transformToLocalize)
    {
        if (!currentlyBoarded)
        {
            ConsoleControl.Write($"Onboarding");

            EnableAudio("Camera.main",Camera.main, false);
            EnableAudio("positionCamera",positionCamera, true);

            ConsoleControl.Write($"Listeners reconfigured");
            if (transformToLocalize != null)
            {
                transformToLocalize.parent = cockpitRoot;
                transformToLocalize.localPosition = Vector3.zero;
                transformToLocalize.localEulerAngles = Vector3.zero;
            }

            screen.isEnabled = currentlyBoarded = isBoarded = true;

        }
    }

    private void EnableAudio(string name, Component t, bool enable)
    {
        if (t == null)
        {
            ConsoleControl.Write($"Trying to switch audio listener of null, read from {name}");
            return;
        }
        var audio = t.GetComponent<AudioListener>();
        if (audio != null)
        {
            ConsoleControl.Write($"Switching audio listener of {t.name} to {enable}");
            audio.enabled = enable;
        }
        else
            ConsoleControl.Write($"No audio listener found on {t.name}");
    }

    public void Offboard()
    {
        if (currentlyBoarded)
        {
            ConsoleControl.Write($"Offboarding");
            EnableAudio("positionCamera", positionCamera, false);
            EnableAudio("Camera.main", Camera.main, true);

            screen.isEnabled = currentlyBoarded = isBoarded = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        look = GetComponent<PointNoseInDirection>();
        screen = GetComponentInChildren<ScreenControl>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentlyBoarded != isBoarded)
        {
            if (!isBoarded)
                Offboard();
            else
                Onboard(null);
        }


        positionCamera.positionBelowTarget = positionCameraBelowSub;
        if (currentlyBoarded)
        {
            rotateCamera.enabled = true;

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

            nonCameraOrientation.rightRotationSpeed = rightAxis * rotationDegreesPerSecond;
            nonCameraOrientation.upRotationSpeed = -upAxis * rotationDegreesPerSecond;


            forwardLeft.thrust = forwardAxis + look.HorizontalRotationIntent * 0.001f;
            forwardRight.thrust = forwardAxis - look.HorizontalRotationIntent * 0.001f;

            positionCamera.zoomAxis = zoomAxis;
        }
        else
        {
            nonCameraOrientation.isActive = false;
            rotateCamera.enabled = false;
            nonCameraOrientation.rightRotationSpeed = 0;
            nonCameraOrientation.upRotationSpeed = 0;
            positionCamera.zoomAxis = 0;
            forwardLeft.thrust = 0;
            forwardRight.thrust = 0;

        }
        backLeft.thrust = -forwardLeft.thrust;
        backRight.thrust = -forwardRight.thrust;


    }

    void FixedUpdate()
    {
        if (currentlyBoarded)
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
}
