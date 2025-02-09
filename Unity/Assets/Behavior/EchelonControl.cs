using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchelonControl : MonoBehaviour
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
    public bool isDocked;

    public float regularForwardAcceleration = 100000;
    public float overdriveForwardAcceleration = 200000;
    public float strafeAcceleration = 50000;
    public float rotationDegreesPerSecond = 100;
    public float waterDrag = 10;
    public float airDrag = 0.1f;


    public DriveControl forwardFacingLeft;
    public DriveControl backFacingLeft;
    public DriveControl forwardFacingRight;
    public DriveControl backFacingRight;

    private NonCameraOrientation nonCameraOrientation;
    private FallOrientation fallOrientation;
    public Transform trailingCameraContainer;
    public Camera trailingColorCamera;
    public Transform cockpitRoot;

    private RotateCamera rotateCamera;
    private PositionCamera positionCamera;

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

                var error = Camera.main.transform.position - cockpitRoot.position;

                transformToLocalize.position -= error;

            }



            trailingCameraContainer.parent = transform.parent;

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
        var audio = t.GetComponentInChildren<AudioListener>();
        if (audio != null)
        {
            ConsoleControl.Write($"Switching audio listener of {t.name}(->{audio.transform.name}) to {enable}");
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
            trailingCameraContainer.parent = transform;

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        nonCameraOrientation = GetComponent<NonCameraOrientation>();
        rb = GetComponent<Rigidbody>();
        look = GetComponent<PointNoseInDirection>();
        screen = GetComponentInChildren<ScreenControl>();
        rotateCamera = trailingCameraContainer.GetComponent<RotateCamera>();
        positionCamera = trailingCameraContainer.GetComponent<PositionCamera>();
        fallOrientation = GetComponent<FallOrientation>();
        look.targetOrientation = inWaterDirectionSource = new TransformDirectionSource(trailingCameraContainer);
    }

    private static string TN(RenderTexture rt)
    {
        if (rt == null)
            return "null";
        return $"{rt.name}, ptr = {rt.GetNativeTexturePtr()}";
    }

    private IDirectionSource inWaterDirectionSource;

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



        if (Input.GetKeyDown(KeyCode.F7))
        {
            ConsoleControl.Write("Capturing debug information v3");
            
            ConsoleControl.Write($"3rd person camera at {trailingCameraContainer.position}");
            ConsoleControl.Write($"Main camera at {Camera.main.transform.position}");
            ConsoleControl.Write($"Cockpit center at {cockpitRoot.position}");
            ConsoleControl.Write($"Trailing color camera at {trailingColorCamera.transform.position}");
            ConsoleControl.Write($"Trailing color camera is enabled = {trailingColorCamera.enabled}");
            ConsoleControl.Write($"Trailing camera rendering to texture {TN(trailingColorCamera.targetTexture)}");
            ConsoleControl.Write($"Main camera rendering to texture {TN(Camera.main.targetTexture)}");
            ConsoleControl.Write($"Last trailing camera matrix {trailingColorCamera.previousViewProjectionMatrix}");
            ConsoleControl.Write($"Last main camera matrix {Camera.main.previousViewProjectionMatrix}");


            ConsoleControl.Write($"RigidBody.isKinematic="+rb.isKinematic);
            ConsoleControl.Write($"RigidBody.constraints="+rb.constraints);
            ConsoleControl.Write($"RigidBody.collisionDetectionMode=" +rb.collisionDetectionMode);
            ConsoleControl.Write($"RigidBody.drag=" +rb.drag);
            ConsoleControl.Write($"RigidBody.mass=" +rb.mass);
            ConsoleControl.Write($"RigidBody.useGravity=" +rb.useGravity);
            ConsoleControl.Write($"RigidBody.velocity=" +rb.velocity);
            ConsoleControl.Write($"RigidBody.worldCenterOfMass=" +rb.worldCenterOfMass);

        }


        if (isBoarded && !isDocked && rb.isKinematic)
        {
            ConsoleControl.Write($"Switching off kinematic mode");
            rb.isKinematic = false;
        }


        positionCamera.positionBelowTarget = positionCameraBelowSub;
        if (currentlyBoarded && !isDocked)
        {
            rotateCamera.enabled = true;

            if (freeCamera)
            {
                rotateCamera.AbortTransition();
                ChangeState(CameraState.IsFree);
                inWaterDirectionSource = nonCameraOrientation;
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
                            
                            inWaterDirectionSource = new TransformDirectionSource(trailingCameraContainer);

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

            look.targetOrientation = outOfWater
                    ? fallOrientation
                    : inWaterDirectionSource;

            //look.targetOrientation = freeCamera ? nonCameraOrientation.transform : shipTrailingCamera.transform;
            if (outOfWater)
            {
                nonCameraOrientation.rightRotationSpeed = 0;
                nonCameraOrientation.upRotationSpeed = 0;
                backFacingLeft.thrust = 0;
                backFacingRight.thrust = 0;

                backFacingLeft.overdrive = 0;
                backFacingRight.overdrive = 0;
            }
            else
            {
                nonCameraOrientation.rightRotationSpeed = rightAxis * rotationDegreesPerSecond;
                nonCameraOrientation.upRotationSpeed = -upAxis * rotationDegreesPerSecond;
                backFacingLeft.thrust = forwardAxis + look.HorizontalRotationIntent * 0.001f;
                backFacingRight.thrust = forwardAxis - look.HorizontalRotationIntent * 0.001f;


                if (overdriveActive)
                {
                    float overdriveThreshold = regularForwardAcceleration / (overdriveForwardAcceleration + regularForwardAcceleration);
                    if (forwardAxis > overdriveThreshold)
                    {
                        backFacingRight.overdrive =
                        backFacingLeft.overdrive =
                            (forwardAxis - overdriveThreshold) / (1f - overdriveThreshold);
                    }
                    else
                        backFacingLeft.overdrive = backFacingRight.overdrive = 0;
                }
                else
                    backFacingLeft.overdrive = backFacingRight.overdrive = 0;

            }




            positionCamera.zoomAxis = zoomAxis;
        }
        else
        {
            nonCameraOrientation.isActive = false;
            rotateCamera.enabled = false;
            nonCameraOrientation.rightRotationSpeed = 0;
            nonCameraOrientation.upRotationSpeed = 0;
            positionCamera.zoomAxis = 0;
            backFacingLeft.thrust = 0;
            backFacingLeft.thrust = 0;
            look.targetOrientation = fallOrientation;

        }

        look.enabled = (isBoarded || outOfWater) && !isDocked;

        forwardFacingLeft.thrust = -backFacingLeft.thrust;
        forwardFacingRight.thrust = -backFacingRight.thrust;

        rb.drag = outOfWater ? airDrag : waterDrag;

        rb.useGravity = outOfWater && !isDocked;

    }

    void FixedUpdate()
    {
        if (currentlyBoarded && !outOfWater && !isDocked)
        {
            rb.AddRelativeForce(0, 0, forwardAxis * (regularForwardAcceleration + (overdriveActive && forwardAxis > 0 ? overdriveForwardAcceleration : 0)));
            if (!freeCamera)
            {
                //var rAxis = M.FlatNormalized(transform.right);
                rb.AddForce(look.targetOrientation.Right * rightAxis * strafeAcceleration);
                rb.AddForce(look.targetOrientation.Up * upAxis * strafeAcceleration);
            }
        }
    }
}
