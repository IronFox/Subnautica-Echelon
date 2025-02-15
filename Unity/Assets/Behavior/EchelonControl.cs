using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EchelonControl : MonoBehaviour
{
    public KeyCode logStateKey = KeyCode.F7;


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
    public bool cameraCenterIsCockpit;

    public float regularForwardAcc = 400;
    public float overdriveForwardAcc = 800;
    public float strafeAcc = 200;
    public float rotationDegreesPerSecond = 100;
    public float waterDrag = 10;
    public float airDrag = 0.1f;


    public DriveControl forwardFacingLeft;
    public DriveControl backFacingLeft;
    public DriveControl forwardFacingRight;
    public DriveControl backFacingRight;

    public Transform trailSpace;
    public Transform cockpitRoot;
    public Transform headCenter;

    private RotateCamera rotateCamera;
    private PositionCamera positionCamera;
    private NonCameraOrientation nonCameraOrientation;
    private FallOrientation fallOrientation;

    private PointNoseInDirection look;
    private Rigidbody rb;
    private bool currentlyBoarded;

    private Parentage onboardLocalizedTransform;
    private Parentage cameraMove;

    private bool currentCameraCenterIsCockpit;
    private bool cameraIsInTrailspace;

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

    private void MoveCameraToTrailSpace()
    {
        if (!cameraIsInTrailspace)
        {
            cameraIsInTrailspace = true;
            ConsoleControl.Write("Moving camera to trailspace");
            cameraMove = Parentage.FromLocal(Camera.main.transform);

            Camera.main.transform.parent = trailSpace;
            TransformDescriptor.LocalIdentity.ApplyTo(Camera.main.transform);
            ConsoleControl.Write("Moved");
        }
    }

    private void MoveCameraOutOfTrailSpace()
    {
        if (cameraIsInTrailspace)
        {
            cameraIsInTrailspace = false;
            ConsoleControl.Write("Moving camera out of trailspace");
            cameraMove.Restore();
            ConsoleControl.Write("Moved");
        }
    }

    private readonly List<Rigidbody> localizeRigidBodyState = new List<Rigidbody>();
    private readonly List<(string Name, Renderer Renderer)> unhide = new List<(string Nasme, Renderer Renderer)>();
    private void Hide(Transform t, Transform[] branchesNotToHide)
    {
        if (branchesNotToHide.Contains(t))
        {
            ConsoleControl.Write($"Encountered {t.name}. Not hiding this branch");
            return;
        }
        var rs = t.GetComponents<SkinnedMeshRenderer>();    
        if (rs != null && rs.Length > 0)
        {

            ConsoleControl.Write($"Hiding {t.name}");
            foreach (var r in rs)
            {
                if (r != null)
                {
                    unhide.Add((r.name,r));
                    r.enabled = false;
                }
            }
        }
 
        for (int i = 0; i < t.childCount; i++)
            Hide(t.GetChild(i), branchesNotToHide);
    }

    public void Onboard(Transform transformToLocalize, params Transform[] branchesNotToHide)
    {
        if (!currentlyBoarded)
        {
            ConsoleControl.Write($"Onboarding");


            localizeRigidBodyState.Clear();
            if (transformToLocalize != null)
            {
                unhide.Clear();
                Hide(transformToLocalize, branchesNotToHide);

                onboardLocalizedTransform = Parentage.FromLocal(transformToLocalize);

                transformToLocalize.parent = headCenter;
                transformToLocalize.localPosition = Vector3.zero;
                transformToLocalize.localEulerAngles = Vector3.zero;
                var rbs = transformToLocalize.GetComponentsInChildren<Rigidbody>();
                foreach (var rb in rbs)
                {
                    if (!rb.isKinematic)
                    {
                        ConsoleControl.Write($"Disabling onboarding rigid body {rb.name}");
                        rb.isKinematic = true;
                        localizeRigidBodyState.Add(rb);
                    }
                }

                var error = Camera.main.transform.position - headCenter.position;

                transformToLocalize.position -= error;


            }
            cameraIsInTrailspace = false;//just in case
            if (!currentCameraCenterIsCockpit)
                MoveCameraToTrailSpace();

            ConsoleControl.Write($"Offloading trail space");
            trailSpace.parent = transform.parent;

            currentlyBoarded = isBoarded = true;

        }
    }



    public void Offboard()
    {
        if (currentlyBoarded)
        {
            ConsoleControl.Write($"Offboarding");
            try
            {


                MoveCameraOutOfTrailSpace();
                ConsoleControl.Write($"Restoring parentage");
                onboardLocalizedTransform.Restore();

                ConsoleControl.Write($"Reactivating rigid bodies ({localizeRigidBodyState.Count})");
                foreach (var rb in localizeRigidBodyState)
                {
                    if (rb != null)
                    {
                        ConsoleControl.Write($"Reactivating {rb.name}");
                        rb.isKinematic = false;
                    }
                    else
                        ConsoleControl.Write($"Lost inactive");
                }

                ConsoleControl.Write($"Unhiding renderers ({unhide.Count})");
                foreach (var r in unhide)
                {
                    if (r.Renderer != null)
                    {
                        ConsoleControl.Write($"Unhiding {r.Renderer.name}");
                        r.Renderer.enabled = true;
                    }
                    else
                        ConsoleControl.Write($"Lost invisible {r.Name}");
                }

                ConsoleControl.Write($"Cleaning up state");
            }
            finally
            {
                currentlyBoarded = isBoarded = false;
                ConsoleControl.Write($"Reintegration trail space");
                trailSpace.parent = transform;
            }

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        nonCameraOrientation = GetComponent<NonCameraOrientation>();
        rb = GetComponent<Rigidbody>();
        look = GetComponent<PointNoseInDirection>();
        rotateCamera = trailSpace.GetComponent<RotateCamera>();
        positionCamera = trailSpace.GetComponent<PositionCamera>();
        fallOrientation = GetComponent<FallOrientation>();
        look.targetOrientation = inWaterDirectionSource = new TransformDirectionSource(trailSpace);
    }

    private static string TN(RenderTexture rt)
    {
        if (rt == null)
            return "null";
        return $"{rt.name}, ptr = {rt.GetNativeTexturePtr()}";
    }


    private static string AllMessages(Exception ex)
    {
        string rs = ex.Message;
        if (ex.InnerException != null)
            rs += "<-" + AllMessages(ex.InnerException);
        return rs;
    }
    
    private void LogComposition(Transform t, Indent indent = default)
    {
        new HierarchyAnalyzer().LogToJson(t, $@"C:\Temp\Logs\snapshot{DateTime.Now:yyyy-MM-dd HH_mm_ss}.json");

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
                Onboard(null, null);
        }

        if (currentCameraCenterIsCockpit != cameraCenterIsCockpit)
        {
            currentCameraCenterIsCockpit = cameraCenterIsCockpit;
            if (currentCameraCenterIsCockpit)
                MoveCameraOutOfTrailSpace();
            else
                MoveCameraToTrailSpace();
        }


        if (Input.GetKeyDown(logStateKey))
        {
            ConsoleControl.Write("Capturing debug information v3");
            
            ConsoleControl.Write($"3rd person camera at {trailSpace.position}");
            ConsoleControl.Write($"Main camera at {Camera.main.transform.position}");
            ConsoleControl.Write($"Cockpit center at {cockpitRoot.position}");


            ConsoleControl.Write($"RigidBody.isKinematic="+rb.isKinematic);
            ConsoleControl.Write($"RigidBody.constraints="+rb.constraints);
            ConsoleControl.Write($"RigidBody.collisionDetectionMode=" +rb.collisionDetectionMode);
            ConsoleControl.Write($"RigidBody.drag=" +rb.drag);
            ConsoleControl.Write($"RigidBody.mass=" +rb.mass);
            ConsoleControl.Write($"RigidBody.useGravity=" +rb.useGravity);
            ConsoleControl.Write($"RigidBody.velocity=" +rb.velocity);
            ConsoleControl.Write($"RigidBody.worldCenterOfMass=" +rb.worldCenterOfMass);

            LogComposition(transform);

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
                            
                            inWaterDirectionSource = new TransformDirectionSource(trailSpace);

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
                    float overdriveThreshold = regularForwardAcc / (overdriveForwardAcc + regularForwardAcc);
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

    bool warnedAboutNoRb = false;
    int roll = 0;
    void FixedUpdate()
    {
        if (currentlyBoarded && !outOfWater && !isDocked)
        {
            if (rb == null)
            {
                if (!warnedAboutNoRb)
                {
                    warnedAboutNoRb = true;
                    ConsoleControl.Write($"Warning: rb is null. Cannot move");
                }
                return;
            }
            var forwardAccel = forwardAxis * (regularForwardAcc + (overdriveActive && forwardAxis > 0 ? overdriveForwardAcc : 0));

            //forwardAccel = forwardAxis * 1e10f;  //turbo debug
            bool log = ((roll++) % 100) == 0;
            if (log)
                ConsoleControl.Write($"Accel: {forwardAccel} {freeCamera} {rb.name}, fps: {1f/Time.fixedTime}");
            try
            {
                rb.AddRelativeForce(0, 0, forwardAccel, ForceMode.Acceleration);
                if (!freeCamera)
                {
                    //var rAxis = M.FlatNormalized(transform.right);
                    rb.AddForce(look.targetOrientation.Right * rightAxis * strafeAcc, ForceMode.Acceleration);
                    rb.AddForce(look.targetOrientation.Up * upAxis * strafeAcc, ForceMode.Acceleration);
                }
                if (log)
                    ConsoleControl.Write($"Done: {forwardAccel} {rb.name}, fps: {1f / Time.fixedTime}");
            }
            catch (Exception ex)
            {
                ConsoleControl.WriteException("FixedUpdate()",ex);
            }
        }
    }
}
