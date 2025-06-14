using System;
using System.Collections.Generic;
using UnityEngine;

public class EchelonControl : MonoBehaviour
{
    public KeyCode openConsoleKey = KeyCode.F7;

    public GameObject targetMarkerPrefab;
    public GameObject targetDirectionMarkerPrefab;
    public GameObject explosionPrefab;
    public float forwardAxis;
    public float rightAxis;
    public float upAxis;
    public float zoomAxis;
    public float lookRightAxis;
    public float lookUpAxis;

    public float targetMarkerSizeScale = 1;

    public int torpedoMark; //0 = disabled, 1 = Mk1, 2 = Mk2, 3 = Mk3
    public int railgunMark = 1;

    /*
    Frequency = 60 * Mathf.Pow(2, mk);
        ExplosionRadius = 15f/4 * Mathf.Pow(2,mk);
        Damage = 1500f / 4 * Mathf.Pow(2, mk);
    */


    public bool overdriveActive;
    public bool outOfWater;
    public bool freeCamera;
    public bool isBoarded;
    public bool positionCameraBelowSub;
    public bool isDocked;
    public bool cameraCenterIsCockpit;
    public bool powerOff;
    public bool batteryDead;
    public bool openUpgradeCover;
    private DateTime lastOnboarded;

    private readonly FloatTimeFrame energyHistory = new FloatTimeFrame(TimeSpan.FromSeconds(2));
    public float maxEnergy = 1;
    public float currentEnergy = 0.5f;
    public float maxHealth = 1;
    public float currentHealth = 0.5f;
    public bool isHealing;

    public TorpedoLaunchControl leftLaunch;
    public TorpedoLaunchControl rightLaunch;

    private TargetProcessor targetProcessor;
    //private bool firingLeft = true;
    private CoverAnimation upgradeCoverAnimation;
    private FirstPersonMarkers firstPersonMarkers;
    private Railgun railgun;
    private CameraShake cameraShake;

    private RailgunTriggerGuidance RailgunGuidance { get; set; }
    private TorpedoTriggerGuidance TorpedoGuidance { get; set; }

    public bool RailgunIsCharging => activeWeapon == Weapon.Railgun && RailgunGuidance.IsCharging;
    public bool RailgunIsDischarging => /*activeWeapon == Weapon.Railgun && */RailgunGuidance.IsDischarging;
    public float regularForwardAcc = 400;
    public float overdriveForwardAcc = 800;
    public float strafeAcc = 200;
    public float rotationDegreesPerSecond = 50;
    public float waterDrag = 10;
    public float airDrag = 0.1f;
    public bool triggerActive;  //captures continuous holding
    public bool triggerWasActivated; //captures trigger key down
    public Weapon activeWeapon = Weapon.Railgun;


    private EnergyLevel energyLevel;

    private Transform cameraRoot;
    private TargetScanner scanner;

    public DriveControl forwardFacingLeft;
    public DriveControl backFacingLeft;
    public DriveControl forwardFacingRight;
    public DriveControl backFacingRight;

    public HealingLight[] healingLights;

    public Transform trailSpace;
    public Transform trailSpaceCameraContainer;
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

    public int ActiveWeaponMark => ActiveGuidance?.Mark ?? 0;

    private void ChangeState(CameraState state)
    {
        this.state = state;
    }

    private void MoveCameraToTrailSpace()
    {
        if (!cameraIsInTrailspace)
        {
            cameraIsInTrailspace = true;
            ULog.Write("Moving camera to trailspace. Setting secondary fallback camera transform");

            CameraUtil.secondaryFallbackCameraTransform = trailSpaceCameraContainer;

            cameraMove = Parentage.FromLocal(cameraRoot);
            cameraRoot.parent = trailSpaceCameraContainer;
            TransformDescriptor.LocalIdentity.ApplyTo(cameraRoot);
            ULog.Write("Moved");
        }
    }

    private void MoveCameraOutOfTrailSpace()
    {
        if (cameraIsInTrailspace)
        {
            cameraIsInTrailspace = false;

            ULog.Write("Moving camera out of trailspace. Unsetting secondary fallback camera transform");

            CameraUtil.secondaryFallbackCameraTransform = null;

            cameraMove.Restore();
            ULog.Write("Moved");
        }
    }



    public void Onboard(Transform localizeInsteadOfMainCamera = null)
    {
        wasEverBoarded = true;
        lastOnboarded = DateTime.Now;
        if (!currentlyBoarded)
        {
            ULog.Write($"Onboarding");

            var listeners = BoardingListeners.Of(this, trailSpace);

            listeners.SignalOnboardingBegin();

            cameraRoot = localizeInsteadOfMainCamera;
            if (cameraRoot == null)
                cameraRoot = Camera.main.transform;
            ULog.Write($"Setting {cameraRoot} as cameraRoot");
            CameraUtil.primaryFallbackCameraTransform = cameraRoot;
            onboardLocalizedTransform = Parentage.FromLocal(cameraRoot);

            cameraIsInTrailspace = false;//just in case
            if (!currentCameraCenterIsCockpit)
                MoveCameraToTrailSpace();

            ULog.Write($"Offloading trail space");
            trailSpace.parent = transform.parent;

            currentlyBoarded = isBoarded = true;

            listeners.SignalOnboardingEnd();
        }
    }



    public void Offboard()
    {
        if (currentlyBoarded)
        {
            ULog.Write($"Offboarding");
            var listeners = BoardingListeners.Of(this, trailSpace);
            try
            {

                listeners.SignalOffBoardingBegin();

                MoveCameraOutOfTrailSpace();
                ULog.Write($"Restoring parentage");
                onboardLocalizedTransform.Restore();
            }
            finally
            {
                currentlyBoarded = isBoarded = false;
                ULog.Write($"Reintegration trail space");
                trailSpace.parent = transform;
            }

            listeners.SignalOffBoardingEnd();

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraShake = trailSpace.GetComponentInChildren<CameraShake>();
        targetProcessor = GetComponent<TargetProcessor>();
        upgradeCoverAnimation = GetComponentInChildren<CoverAnimation>();
        scanner = trailSpace.GetComponentInChildren<TargetScanner>();
        nonCameraOrientation = GetComponent<NonCameraOrientation>();
        rb = GetComponent<Rigidbody>();
        look = GetComponent<DirectAt>();
        rotateCamera = trailSpace.GetComponent<RotateCamera>();
        positionCamera = trailSpace.GetComponent<PositionCamera>();
        fallOrientation = GetComponent<FallOrientation>();
        energyLevel = GetComponentInChildren<EnergyLevel>();
        firstPersonMarkers = GetComponentInChildren<FirstPersonMarkers>();
        railgun = GetComponentInChildren<Railgun>();
        RailgunGuidance = new RailgunTriggerGuidance(statusConsole, firstPersonMarkers, railgun);
        TorpedoGuidance = new TorpedoTriggerGuidance(statusConsole, firstPersonMarkers, leftLaunch, rightLaunch);
        if (look != null)
            look.targetOrientation = inWaterDirectionSource = nonRailgunInWaterDirectionSource = new TransformDirectionSource(trailSpace);
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


    void LateUpdate()
    {
        if (freeTargetMarker != null)
        {
            var camera = CameraUtil.GetTransform(nameof(EchelonControl));
            if (camera != null)
            {
                var ray = new Ray(camera.position, camera.forward);
                freeTargetMarker.MoveTo(ray.GetPoint(((PositionTargetable)freeTargetMarker.Target).AtDistance));

            }
        }
    }

    private ITargetable GetTarget()
    {

        var t = scanner.GetBestTarget(targetProcessor.Latest);
        if (t != null)
        {
            var target = new AdapterTargetable(t);
            return target;
        }
        var camera = CameraUtil.GetTransform(nameof(EchelonControl));
        if (camera != null)
        {
            var ray = new Ray(camera.position, camera.forward);
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
                return new PositionTargetable(targetLocation.Value, closestAt);
            return new PositionTargetable(ray.GetPoint(500f), 500);
        }
        return null;
    }


    private IDirectionSource inWaterDirectionSource;
    private IDirectionSource nonRailgunInWaterDirectionSource;
    private TargetMarker freeTargetMarker;
    private readonly TargetPool<TargetMarker> targetMarkers;
    private readonly TargetPool<TargetDirectionMarker> targetDirectionMarkers;
    private ITargetable lastValidTarget;

    public static TargetArrows targetArrows = TargetArrows.DangerousAndCriticialTargets;
    public static TargetDisplay markerDisplay = TargetDisplay.All;

    public EchelonControl()
    {
        targetMarkers = new TargetPool<TargetMarker>(
            (tm, immediately) => tm.Destroy(immediately),
            ta => TargetMarker.Make(targetMarkerPrefab, ta, this)
            );
        targetDirectionMarkers = new TargetPool<TargetDirectionMarker>(
            (tm, immediately) => tm.Destroy(immediately),
            ta => TargetDirectionMarker.Make(targetDirectionMarkerPrefab, ta, this)
            );
    }

    private bool OnboardingCooldown => DateTime.Now - lastOnboarded < TimeSpan.FromSeconds(1);
    public float SizeOf(ITargetable t)
    {

        var vec = t.GlobalSize * 1.5f;
        var s = Mathf.Max(vec.x, vec.y, vec.z);

        var camera = CameraUtil.GetTransform(nameof(EchelonControl) + '.' + nameof(SizeOf));
        if (camera != null)
            s = M.Max(s * targetMarkerSizeScale, 0.1f * M.Distance(t.Position, camera.position));

        return s;
    }

    private bool coverWasOpen = true;   //call SignalCoverClosed() at start
    public ITargetable liveTarget;

    private void ProcessUpgradeCover()
    {
        if (openUpgradeCover)
        {
            if (upgradeCoverAnimation.IsAtBeginning)
            {
                var hideOnCoverOpen = GetComponentsInChildren<HideIfModuleCoverClosed>();
                foreach (var c in hideOnCoverOpen)
                    c.SignalCoverOpening();
            }
            else
                coverWasOpen = true;
            upgradeCoverAnimation.animateForward = true;
        }
        else
        {
            upgradeCoverAnimation.animateForward = false;
            if (upgradeCoverAnimation.IsAtBeginning)
            {
                if (coverWasOpen)
                {
                    coverWasOpen = false;
                    var hideOnCoverOpen = GetComponentsInChildren<HideIfModuleCoverClosed>();
                    foreach (var c in hideOnCoverOpen)
                        c.SignalCoverClosed();
                }
            }
        }

    }

    public void SelfDestruct(bool pseudo)
    {
        Offboard();
        var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        var control = explosion.GetComponentInChildren<ExplosionController>();
        control.explosionDamage = 100;
        if (pseudo)
        {
            Update();   //do single update to forward alls states
            enabled = false;
            Renderer[] r = GetComponentsInChildren<Renderer>();
            foreach (var c in r)
                c.enabled = false;
        }
        else
            Destroy(gameObject);
    }

    private IWeaponTriggerGuidance ActiveGuidance
    {
        get
        {
            switch (activeWeapon)
            {
                case Weapon.Torpedoes:
                    return TorpedoGuidance;
                case Weapon.Railgun:
                    return RailgunGuidance;
                default:
                    return null;
            }
        }
    }


    private IEnumerable<IWeaponTriggerGuidance> AllWeaponGuidances()
    {
        yield return TorpedoGuidance;
        yield return RailgunGuidance;
    }

    public bool IsFiring => (triggerActive || ActiveGuidance?.MaintainedTarget != null)
                            && isBoarded
                            && !isDocked
                            && !cameraCenterIsCockpit
                            && ActiveWeaponMark > 0
                            && !outOfWater
                            && !OnboardingCooldown
                            && !batteryDead
                            && !powerOff;

    private void ProcessTargeting()
    {
        TorpedoGuidance.Mark = torpedoMark;
        RailgunGuidance.Mark = railgunMark;

        targetProcessor.Work =
                           isBoarded
                        && !isDocked
                        && !cameraCenterIsCockpit
                        && !powerOff
                        && !batteryDead
                        ;

        if (isBoarded && !isDocked && !cameraCenterIsCockpit)
        {
            liveTarget = GetTarget();
            statusConsole.Set(StatusProperty.Target, liveTarget);


            IEnumerable<ITargetable> set = targetProcessor.Latest.Targets;
            if (liveTarget != null
                && CanHit(liveTarget.Position)
                && liveTarget is PositionTargetable pt
                )
            {
                if (freeTargetMarker != null)
                    freeTargetMarker.MoveTo(pt.Position);
                else
                {
                    freeTargetMarker = TargetMarker.Make(targetMarkerPrefab, pt, this);
                    freeTargetMarker.Scale(SizeOf(liveTarget));
                    freeTargetMarker.TargetHealthFeed.isPrimary = true;
                    freeTargetMarker.TargetHealthFeed.isLocked = true;
                }
            }
            else
            {
                freeTargetMarker?.Destroy(false);
                freeTargetMarker = null;
            }

            targetMarkers.UpdateAll(set,
                (tm, t) =>
                {
                    tm.MoveTo(t.Position);
                    var targetSize = SizeOf(t);
                    tm.Scale(targetSize);
                    var primary = t.Equals(liveTarget);
                    tm.TargetHealthFeed.isPrimary = primary;
                    tm.TargetHealthFeed.isLocked = primary && CanHit(liveTarget.Position);
                });
            if (targetArrows != TargetArrows.None && !positionCamera.isFirstPerson)
                targetDirectionMarkers.UpdateAll(targetProcessor.Latest.Targets,
                    (tm, t) => { });
            else
                targetDirectionMarkers.Purge();

            TargetListeners.Of(this, trailSpace).SignalNewTarget(this);


            if (liveTarget != null)
            {
                var targetSize = SizeOf(liveTarget);

                lastValidTarget = liveTarget;
            }
            else
            {
                lastValidTarget = null;
            }
            IWeaponTriggerGuidance guidance = ActiveGuidance;
            foreach (var g in AllWeaponGuidances())
                if (g != guidance)
                    g.OnTriggerLost();


            if (guidance != null)
            {
                if (triggerWasActivated)
                {
                    guidance.OnTriggerWasActivated(liveTarget);
                }
                else if (triggerActive)
                {
                    guidance.OnTriggerActiveOn(liveTarget);
                }
                else
                    guidance.OnTriggerInactive();
            }

            if (!triggerActive && guidance.MaintainedTarget != null)
                liveTarget = guidance.MaintainedTarget;


            var doFire = IsFiring;


            foreach (var all in AllWeaponGuidances())
                all.OnUpdate();

        }
        else
        {
            freeTargetMarker?.Destroy(true);
            freeTargetMarker = null;

            leftLaunch.FireWithTarget = null;
            rightLaunch.FireWithTarget = null;
            railgun.FireWithTarget = null;
            statusConsole.Set(StatusProperty.Target, null);
            statusConsole.Set(StatusProperty.LeftLauncherTarget, null);
            statusConsole.Set(StatusProperty.RightLauncherTarget, null);

            targetMarkers.Purge();
            targetDirectionMarkers.Purge();
        }
        statusConsole.Set(StatusProperty.LeftLauncherProgress, leftLaunch.CycleProgress / leftLaunch.CycleTime);
        statusConsole.Set(StatusProperty.RightLauncherProgress, rightLaunch.CycleProgress / rightLaunch.CycleTime);

    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            firstPersonMarkers.overdriveActive = false;
            cameraShake.overdriveActive = false;

            ProcessTargeting();
            ProcessUpgradeCover();


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
            statusConsole.Set(StatusProperty.OpenUpgradeCover, openUpgradeCover);
            statusConsole.Set(StatusProperty.IsFirstPerson, positionCamera.isFirstPerson);

            firstPersonMarkers.show =
                positionCamera.isFirstPerson
                && isBoarded
                && !isDocked
                && !batteryDead
                && !powerOff;

            foreach (var h in healingLights)
                h.isHealing = isHealing;

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

            if (currentlyBoarded != isBoarded)
            {
                if (!isBoarded)
                    Offboard();
                else
                    Onboard();
            }
            else
            {
                if (currentlyBoarded)
                {
                    if (trailSpace.parent != transform.parent)
                    {
                        ULog.Warn($"Trail space is no longer sibling to sub. Moving...");
                        trailSpace.parent = transform.parent;
                    }
                }
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
                }
                else
                    ULog.Write($"Not currently boarded. Ignoring console key");

            }


            rotateCamera.rotationAxisX = lookRightAxis;
            rotateCamera.rotationAxisY = lookUpAxis;

            positionCamera.positionBelowTarget = positionCameraBelowSub;

            if (currentlyBoarded && !isDocked && !cameraCenterIsCockpit)
            {
                rotateCamera.enabled = true;
                if (freeCamera)
                {
                    rotateCamera.AbortTransition();
                    ChangeState(CameraState.IsFree);
                    nonRailgunInWaterDirectionSource = inWaterDirectionSource = nonCameraOrientation;
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

                                nonRailgunInWaterDirectionSource = inWaterDirectionSource = new TransformDirectionSource(trailSpace);

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

                if (activeWeapon == Weapon.Railgun
                    && railgun.WantsTargetOrientation
                    && RailgunGuidance.CanHitWithRotation(railgun.FireWithTarget.Position))
                    inWaterDirectionSource = railgun;
                else
                    inWaterDirectionSource = nonRailgunInWaterDirectionSource;

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
                            firstPersonMarkers.overdriveActive = true;
                            cameraShake.overdriveActive = true;
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
            ULog.Exception($"EchelongControl.Update()", ex, gameObject);
        }
    }

    public void Localize(Transform player)
    {
        player.parent = seat;
        player.localPosition = Vector3.zero;
        player.localEulerAngles = Vector3.zero;
    }

    public bool CanHit(Vector3 position)
    {
        if (ActiveWeaponMark == 0)
            return false;
        return ActiveGuidance.CanHitWithRotation(position);
    }
}

internal class TargetDirectionMarker
{
    public GameObject GameObject { get; }
    public DirectMarkerAtTarget Control { get; }
    public TargetDirectionMarker(GameObject go)
    {
        GameObject = go;
        Control = GameObject.GetComponent<DirectMarkerAtTarget>();
    }
    public void Destroy(bool immediately)
    {
        if (immediately)
            GameObject.Destroy(GameObject);
        else
            Control.target = null;
    }

    internal static TargetDirectionMarker Make(GameObject targetDirectionMarkerPrefab, ITargetable target, EchelonControl echelon)
    {
        var rs = GameObject.Instantiate(targetDirectionMarkerPrefab, target.Position, Quaternion.identity);
        var tm = new TargetDirectionMarker(rs);
        tm.Control.target = target;
        tm.Control.adapterTarget = target as AdapterTargetable;
        tm.Control.echelon = echelon;
        return tm;
    }
}

internal class TargetMarker
{
    public GameObject GameObject { get; }
    public TargetHealthFeed TargetHealthFeed { get; }
    public ITargetable Target { get; }
    public TargetMarker(GameObject go, ITargetable target)
    {
        GameObject = go;
        Target = target;
        TargetHealthFeed = GameObject.GetComponent<TargetHealthFeed>();
    }

    internal static TargetMarker Make(GameObject targetMarkerPrefab, ITargetable target, EchelonControl echelon)
    {
        var rs = GameObject.Instantiate(targetMarkerPrefab, target.Position, Quaternion.identity);
        var tm = new TargetMarker(rs, target);
        tm.TargetHealthFeed.owner = echelon;
        tm.TargetHealthFeed.target = (target as AdapterTargetable)?.TargetAdapter;
        return tm;
    }

    public void Scale(float targetSize)
    {
        GameObject.transform.localScale = M.V3(targetSize);
    }

    public void MoveTo(Vector3 position)
    {
        GameObject.transform.position = position;
    }

    public void Destroy(bool immediately)
    {
        GameObject.Destroy(GameObject);
    }
}

public enum TargetArrows
{
    DangerousAndCriticialTargets,
    CriticalOnly,
    None
}

public enum Weapon
{
    Torpedoes,
    Railgun
}