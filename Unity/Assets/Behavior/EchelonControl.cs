using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EchelonControl : MonoBehaviour
{
    public KeyCode logStateKey = KeyCode.F7;

    public GameObject targetMarkerPrefab;
    public float forwardAxis;
    public float rightAxis;
    public float upAxis;
    public float zoomAxis;
    public float lookRightAxis;
    public float lookUpAxis;

    public bool overdriveActive;
    public bool outOfWater;
    public bool freeCamera;
    public bool isBoarded;
    public bool positionCameraBelowSub;
    public bool isDocked;
    public bool cameraCenterIsCockpit;
    public bool powerOff;
    public bool batteryDead;

    public float maxEnergy;
    public float currentEnergy;
    public float energyChange;

    public MeshRenderer[] lightsRenderers = Array.Empty<MeshRenderer>();

    public TorpedoLaunchControl leftLaunch;
    public TorpedoLaunchControl rightLaunch;

    private bool firingLeft = true;

    public float regularForwardAcc = 400;
    public float overdriveForwardAcc = 800;
    public float strafeAcc = 200;
    public float rotationDegreesPerSecond = 50;
    public float waterDrag = 10;
    public float airDrag = 0.1f;
    public bool triggerActive;
    private DateTime lastTriggerTime = DateTime.MinValue;

    private EnergyLevel energyLevel;

    private Transform cameraRoot;
    private TargetScanner scanner;

    public DriveControl forwardFacingLeft;
    public DriveControl backFacingLeft;
    public DriveControl forwardFacingRight;
    public DriveControl backFacingRight;

    public Transform trailSpace;
    //public Transform cockpitRoot;
    //public Transform headCenter;
    public Transform seat;

    private RotateCamera rotateCamera;
    private PositionCamera positionCamera;
    private NonCameraOrientation nonCameraOrientation;
    private FallOrientation fallOrientation;

    private DirectAt look;
    //private Rigidbody rb;
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
        //Debug.Log($"->{state}");
        this.state = state;
    }

    private void MoveCameraToTrailSpace()
    {
        if (!cameraIsInTrailspace)
        {
            cameraIsInTrailspace = true;
            ConsoleControl.Write("Moving camera to trailspace");
            cameraMove = Parentage.FromLocal(cameraRoot);

            cameraRoot.parent = trailSpace;
            TransformDescriptor.LocalIdentity.ApplyTo(cameraRoot);
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

    public void Onboard(Transform localizeInsteadOfMainCamera = null)
    {
        if (!currentlyBoarded)
        {
            ConsoleControl.Write($"Onboarding");

            cameraRoot = localizeInsteadOfMainCamera;
            if (cameraRoot == null)
                cameraRoot = Camera.main.transform;
            onboardLocalizedTransform = Parentage.FromLocal(cameraRoot);

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
        scanner = trailSpace.GetComponentInChildren<TargetScanner>();
        nonCameraOrientation = GetComponent<NonCameraOrientation>();
        //rb = GetComponent<Rigidbody>();
        look = GetComponent<DirectAt>();
        rotateCamera = trailSpace.GetComponent<RotateCamera>();
        positionCamera = trailSpace.GetComponent<PositionCamera>();
        fallOrientation = GetComponent<FallOrientation>();
        energyLevel = GetComponentInChildren<EnergyLevel>();
        if (look != null)
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

    private ITargetable GetTarget()
    {
        var t = scanner.GetBestTarget(transform);
        if (t != null)
        {
            var target = new AdapterTargetable(t);
            return target;
        }
        var ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        var candidates = Physics.RaycastAll(ray);
        float closestAt = 500;
        Vector3? targetLocation = null;
        foreach (var candidate in candidates)
        {
            if (candidate.collider.gameObject.transform.IsChildOf(transform))
                continue;
            if (candidate.collider.isTrigger || !candidate.collider.enabled)
                continue;
            //if (candidate.rigidbody != null &&
            //    candidate.rigidbody.isKinematic
            //    )
            //    continue;
            var dist = candidate.distance;
            if (dist < closestAt)
            {
                closestAt = dist;
                targetLocation = candidate.point;
            }
        }
        if (targetLocation.HasValue)
            return new PositionTargetable(targetLocation.Value);
        return new PositionTargetable(ray.GetPoint(500f));
    }


    private IDirectionSource inWaterDirectionSource;
    private GameObject targetMarker;
    private TargetHealthFeed targetHealthFeed;
    private ITargetable lastValidTarget;

    private Vector3 SizeOf(ITargetable t)
    {
        var vec = M.Max(t.GlobalSize*2, 0.1f * M.Distance(t.Position, Camera.main.transform.position));
        var s = Mathf.Max(vec.x,vec.y, vec.z);
        return M.V3(s);
    }


    private void ProcessTargeting()
    {
        if (isBoarded && !isDocked)
        {
            var target = GetTarget();
            //ConsoleControl.Write($"target: "+target.ToString());
            if (target != null)
            {
                if (!(target is PositionTargetable) && !target.Equals(lastValidTarget))
                    ConsoleControl.Write($"New target acquired: {target}");

                lastValidTarget = target;
                if (targetMarker == null)
                {
                    ConsoleControl.Write($"Creating target marker");
                    targetMarker = Instantiate(targetMarkerPrefab, target.Position, Quaternion.identity);
                    targetMarker.transform.localScale = SizeOf(target);
                    targetHealthFeed = targetMarker.GetComponent<TargetHealthFeed>();
                    if (targetHealthFeed != null)
                        targetHealthFeed.target = (target as AdapterTargetable)?.TargetAdapter;
                }
                else
                {
                    //Debug.Log($"Repositioning target marker");
                    targetMarker.transform.position = target.Position;
                    targetMarker.transform.localScale = SizeOf(target);
                    if (targetHealthFeed != null)
                        targetHealthFeed.target = (target as AdapterTargetable)?.TargetAdapter;
                }
            }
            else
            {
                ConsoleControl.Write($"Destroying target marker");
                Destroy(targetMarker);
                targetMarker = null;
            }

            var firing = firingLeft ? leftLaunch : rightLaunch;

            var doFire = triggerActive && !outOfWater;
            firing.fireWithTarget = doFire ? target : null;
            if (firing.CycleProgress > firing.CycleTime * 0.5f)
            {
                ConsoleControl.Write($"Switching tube");
                firing.fireWithTarget = null;
                firingLeft = !firingLeft;
            }
        }
        else if (targetMarker != null)
        {
            ConsoleControl.Write($"Destroying target marker");
            Destroy(targetMarker);
            targetMarker = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            ProcessTargeting();

            energyLevel.currentChange = energyChange;
            energyLevel.maxEnergy = maxEnergy;
            energyLevel.currentEnergy = currentEnergy;


            foreach (var r in lightsRenderers)
                r.enabled = !batteryDead && !powerOff;


            if (currentlyBoarded != isBoarded)
            {
                if (!isBoarded)
                    Offboard();
                else
                    Onboard();
            }

            if (currentCameraCenterIsCockpit != cameraCenterIsCockpit && currentlyBoarded)
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
                ConsoleControl.Write($"Main camera at {cameraRoot.position}");
                //ConsoleControl.Write($"Cockpit center at {cockpitRoot.position}");


                //ConsoleControl.Write($"RigidBody.isKinematic="+rb.isKinematic);
                //ConsoleControl.Write($"RigidBody.constraints="+rb.constraints);
                //ConsoleControl.Write($"RigidBody.collisionDetectionMode=" +rb.collisionDetectionMode);
                //ConsoleControl.Write($"RigidBody.drag=" +rb.drag);
                //ConsoleControl.Write($"RigidBody.mass=" +rb.mass);
                //ConsoleControl.Write($"RigidBody.useGravity=" +rb.useGravity);
                //ConsoleControl.Write($"RigidBody.velocity=" +rb.velocity);
                //ConsoleControl.Write($"RigidBody.worldCenterOfMass=" +rb.worldCenterOfMass);

                LogComposition(transform);

            }


            rotateCamera.rotationAxisX = lookRightAxis;
            rotateCamera.rotationAxisY = lookUpAxis;

            positionCamera.positionBelowTarget = positionCameraBelowSub;

            if (currentlyBoarded && !isDocked)
            {
                rotateCamera.enabled = true;

                if (freeCamera)
                {
                    rotateCamera.AbortTransition();
                    ChangeState(CameraState.IsFree);
                    inWaterDirectionSource = nonCameraOrientation;
                    if (nonCameraOrientation != null)
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

                                if (nonCameraOrientation != null)
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

                if (look != null)
                    look.targetOrientation = outOfWater
                            ? fallOrientation
                            : inWaterDirectionSource;
                nonCameraOrientation.outOfWater = outOfWater;

                if (outOfWater)
                {
                    if (nonCameraOrientation != null)
                    {
                        nonCameraOrientation.rightRotationSpeed = 0;
                        nonCameraOrientation.upRotationSpeed = 0;
                    }
                    backFacingLeft.thrust = 0;
                    backFacingRight.thrust = 0;

                    backFacingLeft.overdrive = 0;
                    backFacingRight.overdrive = 0;
                }
                else
                {
                    if (nonCameraOrientation != null)
                    {
                        nonCameraOrientation.rightRotationSpeed = rightAxis * rotationDegreesPerSecond;
                        nonCameraOrientation.upRotationSpeed = -upAxis * rotationDegreesPerSecond;
                    }
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
                if (nonCameraOrientation != null)
                {
                    nonCameraOrientation.isActive = false;
                    nonCameraOrientation.rightRotationSpeed = 0;
                    nonCameraOrientation.upRotationSpeed = 0;
                }
                if (isDocked)
                {
                    //trailSpace.transform.forward = transform.forward;
                    rotateCamera.CopyOrientationFrom(transform);
                }

                rotateCamera.enabled = false;
                positionCamera.zoomAxis = 0;
                backFacingLeft.thrust = 0;
                backFacingRight.thrust = 0;
                if (look != null)
                    look.targetOrientation = fallOrientation;

            }

            if (look != null)
                look.enabled = (isBoarded || outOfWater) && !isDocked && !batteryDead && !powerOff;

            forwardFacingLeft.thrust = -backFacingLeft.thrust;
            forwardFacingRight.thrust = -backFacingRight.thrust;

            //rb.drag = outOfWater ? airDrag : waterDrag;

            //rb.useGravity = outOfWater && !isDocked;
        }
        catch (Exception ex)
        {
            ConsoleControl.WriteException($"EchelongControl.Update()", ex);
        }
    }

    public void Localize(Transform player)
    {
        player.parent = seat;
        player.localPosition = Vector3.zero;
        player.localEulerAngles = Vector3.zero;
    }

    //bool warnedAboutNoRb = false;
    //int roll = 0;
    //void FixedUpdate()
    //{
    //    if (currentlyBoarded && !outOfWater && !isDocked)
    //    {
    //        if (rb == null)
    //        {
    //            if (!warnedAboutNoRb)
    //            {
    //                warnedAboutNoRb = true;
    //                ConsoleControl.Write($"Warning: rb is null. Cannot move");
    //            }
    //            return;
    //        }
    //        var forwardAccel = forwardAxis * (regularForwardAcc + (overdriveActive && forwardAxis > 0 ? overdriveForwardAcc : 0));

    //        //forwardAccel = forwardAxis * 1e10f;  //turbo debug
    //        bool log = ((roll++) % 100) == 0;
    //        if (log)
    //            ConsoleControl.Write($"Accel: {forwardAccel} {freeCamera} {rb.name}, fps: {1f/Time.fixedTime}");
    //        try
    //        {
    //            rb.AddRelativeForce(0, 0, forwardAccel, ForceMode.Acceleration);
    //            if (!freeCamera)
    //            {
    //                //var rAxis = M.FlatNormalized(transform.right);
    //                rb.AddForce(look.targetOrientation.Right * rightAxis * strafeAcc, ForceMode.Acceleration);
    //                rb.AddForce(look.targetOrientation.Up * upAxis * strafeAcc, ForceMode.Acceleration);
    //            }
    //            if (log)
    //                ConsoleControl.Write($"Done: {forwardAccel} {rb.name}, fps: {1f / Time.fixedTime}");
    //        }
    //        catch (Exception ex)
    //        {
    //            ConsoleControl.WriteException("FixedUpdate()",ex);
    //        }
    //    }
    //}
}
