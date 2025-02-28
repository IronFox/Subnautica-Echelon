using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EchelonControl : MonoBehaviour
{
    public KeyCode openConsoleKey = KeyCode.F7;

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
    private DateTime lastOnboarded;

    private readonly FloatTimeFrame energyHistory = new FloatTimeFrame(TimeSpan.FromSeconds(2));
    public float maxEnergy=1;
    public float currentEnergy=0.5f;
    public float maxHealth = 1;
    public float currentHealth = 0.5f;
    public bool isHealing;

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

    public HealingLight healingLight;

    public Transform trailSpace;
    public Transform trailSpaceCameraContainer;
    //public Canvas trailSpaceCanvas;
    //public Transform cockpitRoot;
    //public Transform headCenter;
    public Transform seat;
    public StatusConsole statusConsole;

    private RotateCamera rotateCamera;
    private PositionCamera positionCamera;
    private NonCameraOrientation nonCameraOrientation;
    private FallOrientation fallOrientation;

    private DirectAt look;
    private Rigidbody rb;
    private bool currentlyBoarded;

    private Parentage onboardLocalizedTransform;
    private Parentage cameraMove;

    private bool currentCameraCenterIsCockpit;
    private bool cameraIsInTrailspace;

    private bool wasEverBoarded;
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
            cameraRoot.parent = trailSpaceCameraContainer;

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



    public void Onboard(Transform localizeInsteadOfMainCamera = null)
    {
        wasEverBoarded = true;
        lastOnboarded = DateTime.Now;
        if (!currentlyBoarded)
        {
            ConsoleControl.Write($"Onboarding");

            var listeners = BoardingListeners.Of(this, trailSpace);

            listeners.SignalOnboardingBegin();

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

            listeners.SignalOnboardingEnd();
        }
    }



    public void Offboard()
    {
        if (currentlyBoarded)
        {
            ConsoleControl.Write($"Offboarding");
            var listeners = BoardingListeners.Of(this, trailSpace);
            try
            {

                listeners.SignalOffBoardingBegin();

                MoveCameraOutOfTrailSpace();
                ConsoleControl.Write($"Restoring parentage");
                onboardLocalizedTransform.Restore();
            }
            finally
            {
                currentlyBoarded = isBoarded = false;
                ConsoleControl.Write($"Reintegration trail space");
                trailSpace.parent = transform;
            }

            listeners.SignalOffBoardingEnd();

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        scanner = trailSpace.GetComponentInChildren<TargetScanner>();
        nonCameraOrientation = GetComponent<NonCameraOrientation>();
        rb = GetComponent<Rigidbody>();
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

    private bool OnboardingCooldown => DateTime.Now - lastOnboarded < TimeSpan.FromSeconds(1);
    private float SizeOf(ITargetable t)
    {
        var vec = M.Max(t.GlobalSize*2, 0.1f * M.Distance(t.Position, Camera.main.transform.position));
        var s = Mathf.Max(vec.x,vec.y, vec.z);
        return s;
    }


    private void ProcessTargeting()
    {
        if (isBoarded && !isDocked)
        {
            var target = GetTarget();
            statusConsole.Set(StatusProperty.Target, target);
            //ConsoleControl.Write($"target: "+target.ToString());
            if (target != null)
            {
                var targetSize = SizeOf(target);
                TargetListeners.Of(this, trailSpace).SignalNewTarget(target, targetSize);

                if (!(target is PositionTargetable) && !target.Equals(lastValidTarget))
                    ConsoleControl.Write($"New target acquired: {target}");

                lastValidTarget = target;
                if (targetMarker == null)
                {
                    ConsoleControl.Write($"Creating target marker");
                    targetMarker = Instantiate(targetMarkerPrefab, target.Position, Quaternion.identity);
                    targetMarker.transform.localScale = M.V3(targetSize);
                    targetHealthFeed = targetMarker.GetComponent<TargetHealthFeed>();
                    targetHealthFeed.owner = this;
                    if (targetHealthFeed != null)
                        targetHealthFeed.target = (target as AdapterTargetable)?.TargetAdapter;
                }
                else
                {
                    //Debug.Log($"Repositioning target marker");
                    targetMarker.transform.position = target.Position;
                    targetMarker.transform.localScale = M.V3(targetSize);
                    if (targetHealthFeed != null)
                        targetHealthFeed.target = (target as AdapterTargetable)?.TargetAdapter;
                }
            }
            else
            {
                if (lastValidTarget != null)
                    TargetListeners.Of(this, trailSpace).SignalNewTarget(null, 1);
                ConsoleControl.Write($"Destroying target marker");
                Destroy(targetMarker);
                targetMarker = null;
                lastValidTarget = null;
            }

            var firing = firingLeft ? leftLaunch : rightLaunch;

            var doFire = triggerActive && !outOfWater && !OnboardingCooldown;

            // Debug.Log($"doFire={doFire} (triggerActive={triggerActive}, outOfWater={outOfWater})");

            firing.fireWithTarget = doFire ? target : null;
            if (firing.CycleProgress > firing.CycleTime * 0.5f)
            {
                ConsoleControl.Write($"Switching tube");
                firing.fireWithTarget = null;
                firingLeft = !firingLeft;
            }
            statusConsole.Set(StatusProperty.LeftLauncherTarget, leftLaunch.fireWithTarget);
            statusConsole.Set(StatusProperty.RightLauncherTarget, rightLaunch.fireWithTarget);
        }
        else
        {
            leftLaunch.fireWithTarget = null;
            rightLaunch.fireWithTarget = null;
            statusConsole.Set(StatusProperty.Target, null);
            statusConsole.Set(StatusProperty.LeftLauncherTarget,null);
            statusConsole.Set(StatusProperty.RightLauncherTarget, null);
            if (targetMarker != null)
            {
                ConsoleControl.Write($"Destroying target marker");
                Destroy(targetMarker);
                targetMarker = null;
            }
        }
        statusConsole.Set(StatusProperty.LeftLauncherProgress, leftLaunch.CycleProgress / leftLaunch.CycleTime);
        statusConsole.Set(StatusProperty.RightLauncherProgress, rightLaunch.CycleProgress / rightLaunch.CycleTime);

    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            ProcessTargeting();
            
            statusConsole.Set(StatusProperty.EnergyLevel, currentEnergy);
            statusConsole.Set(StatusProperty.EnergyCapacity, maxEnergy);
            statusConsole.Set(StatusProperty.BatteryDead, batteryDead);
            statusConsole.Set(StatusProperty.PowerOff, powerOff);
            statusConsole.Set(StatusProperty.IsBoarded, currentlyBoarded);
            statusConsole.Set(StatusProperty.IsOutOfWater, outOfWater);
            statusConsole.Set(StatusProperty.LookRightAxis, lookRightAxis);
            statusConsole.Set(StatusProperty.LookUpAxis, lookUpAxis);
            statusConsole.Set(StatusProperty.ForwardAxis, forwardAxis);
            statusConsole.Set(StatusProperty.RightAxis, rightAxis);
            statusConsole.Set(StatusProperty.UpAxis, upAxis);
            statusConsole.Set(StatusProperty.OverdriveActive, overdriveActive);
            statusConsole.Set(StatusProperty.CameraDistance, positionCamera.DistanceToTarget);
            statusConsole.Set(StatusProperty.PositionCameraBelowSub, positionCamera.positionBelowTarget);
            statusConsole.Set(StatusProperty.Velocity, rb.velocity.magnitude);
            statusConsole.Set(StatusProperty.FreeCamera, freeCamera);
            statusConsole.Set(StatusProperty.IsDocked, isDocked);
            statusConsole.Set(StatusProperty.TimeDelta, Time.deltaTime);
            statusConsole.Set(StatusProperty.FixedTimeDelta, Time.fixedDeltaTime);
            statusConsole.Set(StatusProperty.TargetScanTime, scanner.lastScanTime);
            statusConsole.Set(StatusProperty.Health, currentHealth);
            statusConsole.Set(StatusProperty.MaxHealth, maxHealth);
            statusConsole.Set(StatusProperty.IsHealing, isHealing);
            statusConsole.Set(StatusProperty.TriggerActive, triggerActive);
            statusConsole.Set(StatusProperty.OnboardingCooldown, OnboardingCooldown);

            healingLight.isHealing = isHealing;

            energyHistory.Add(currentEnergy);
            var edge = energyHistory.GetEdge();
            if (edge.HasValue)
            {
                float energyChange = (currentEnergy - edge.Value) * 5f;
                energyLevel.currentChange = energyChange;
            }
            else
                energyLevel.currentChange = 0;

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

            if (Input.GetKeyDown(openConsoleKey))
            {
                if (currentlyBoarded)
                {
                    statusConsole.ToggleVisibility();

                    //ConsoleControl.Write("Capturing debug information v3");

                    //ConsoleControl.Write($"3rd person camera at {trailSpace.position}");
                    //ConsoleControl.Write($"Main camera at {cameraRoot.position}");
                    ////ConsoleControl.Write($"Cockpit center at {cockpitRoot.position}");


                    ////ConsoleControl.Write($"RigidBody.isKinematic="+rb.isKinematic);
                    ////ConsoleControl.Write($"RigidBody.constraints="+rb.constraints);
                    ////ConsoleControl.Write($"RigidBody.collisionDetectionMode=" +rb.collisionDetectionMode);
                    ////ConsoleControl.Write($"RigidBody.drag=" +rb.drag);
                    ////ConsoleControl.Write($"RigidBody.mass=" +rb.mass);
                    ////ConsoleControl.Write($"RigidBody.useGravity=" +rb.useGravity);
                    ////ConsoleControl.Write($"RigidBody.velocity=" +rb.velocity);
                    ////ConsoleControl.Write($"RigidBody.worldCenterOfMass=" +rb.worldCenterOfMass);

                    //LogComposition(transform);

                }
                else
                    ConsoleControl.Write($"Not currently boarded. Ignoring console key");

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
                look.enabled = (isBoarded || (outOfWater && wasEverBoarded)) && !isDocked && !batteryDead && !powerOff;

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

}
