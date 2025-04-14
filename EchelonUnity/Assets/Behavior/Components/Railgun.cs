using UnityEngine;

public class Railgun : MonoBehaviour, IDirectionSource
{
    public EchelonControl echelon;
    public GameObject shotPrefab;
    private RailgunShot shot;
    public CoverAnimation openCoverAnimation;
    public float damage = 2000;
    public float speedMetersPerSecond = 800;
    public float AngleTolerance { get; set; } = 5;
    private TargetPredictor targetPredictor;
    private Vector3 forward = Vector3.forward;

    public ITargetable FireWithTarget { get; set; }
    public bool holdFireOnBadAim = false;
    public Vector3 Forward => forward;

    private Vector3 CalculateInterceptDirection()
    {
        var pred = targetPredictor.CurentPrediction;
        if (pred is null)
            return transform.forward;

        return M.Intercept(pred.Value, transform.position, speedMetersPerSecond);
    }

    public Vector3 Right => -Vector3.Cross(Forward, Vector3.up);

    public Vector3 Up => Vector3.up;

    public float Impact => FireWithTarget?.Exists == true ? 1 : 0;

    public float ZImpact => Impact * 0.1f;

    public bool WantsTargetOrientation => FireWithTarget?.Exists == true;
    public int mark = 1;
    public bool CurrentShotIsDone { get; private set; }
    public bool IsCharging => FireWithTarget?.Exists == true && shot && shot.IsCharging;
    public bool IsDischarging => shot && shot.IsDischarging;

    public bool CanHitWithoutRotation(Vector3 position)
    {
        var a = Vector3.Angle(position - transform.position, transform.forward);
        return a <= AngleTolerance;
    }
    public float AngleError(Vector3 position)
        => Vector3.Angle(position - transform.position, transform.forward);

    public Vector3 ClosestHitDirection
    {
        get
        {
            var a = Vector3.Angle(forward, transform.forward);
            if (a <= AngleTolerance)
                return forward;
            else
            {
                var axis = Vector3.Cross(forward, transform.forward);
                var clamped = Quaternion.AngleAxis(5, -axis.normalized) * transform.forward;
                return clamped;
            }

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        targetPredictor = GetComponent<TargetPredictor>();
    }

    // Update is called once per frame
    void Update()
    {
        targetPredictor.target = FireWithTarget;
        forward = CalculateInterceptDirection();
        var doFire = FireWithTarget?.Exists == true;
        if (doFire)
        {
            openCoverAnimation.animateForward = true;
            if (openCoverAnimation.IsAtEnd)
            {
                if (!shot)
                {
                    var instance = Instantiate(shotPrefab, transform);

                    instance.transform.rotation = Quaternion.LookRotation(ClosestHitDirection);

                    instance.transform.localScale = Vector3.one;
                    instance.transform.localPosition = Vector3.zero;
                    shot = instance.GetComponent<RailgunShot>();
                    shot.owner = echelon;
                    shot.damage = damage;
                    shot.upgradeLevel = mark;
                    shot.speedMetersPerSecond = speedMetersPerSecond;
                }
            }
        }
        else
            CurrentShotIsDone = false;


        if (shot)
        {
            shot.canFire = !holdFireOnBadAim || Vector3.Angle(Forward, transform.forward) < 5;

            if (shot.HasFired)
            {
                shot.transform.SetParent(echelon.transform.parent);
            }
            else
            {
                shot.transform.rotation = Quaternion.LookRotation(ClosestHitDirection);
            }
            shot.doContinue = doFire;
            if (shot.SecondsAfterFired >= 1)
            {
                openCoverAnimation.animateForward = false;
                CurrentShotIsDone = true;
                shot = null;
            }
        }
    }
}
