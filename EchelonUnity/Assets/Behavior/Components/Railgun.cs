using UnityEngine;

public class Railgun : MonoBehaviour, IFirable, IDirectionSource
{
    public EchelonControl echelon;
    public GameObject shotPrefab;
    private RailgunShot shot;
    public CoverAnimation openCoverAnimation;
    public float damage = 2000;
    public float speedMetersPerSecond = 800;
    private TargetPredictor targetPredictor;
    private Vector3 forward = Vector3.forward;

    public ITargetable FireWithTarget { get; set; }

    public Vector3 Forward => forward;

    private Vector3 CalculateInterceptDirection()
    {
        var pred = targetPredictor.CurentPrediction;
        if (pred is null)
            return transform.forward;

        return M.Intercept(pred.Value, new LinearPrediction(transform.forward * speedMetersPerSecond, transform.position));
    }

    public Vector3 Right => -Vector3.Cross(Forward, Vector3.up);

    public Vector3 Up => Vector3.up;

    public float Impact => FireWithTarget?.Exists == true ? 1 : 0;

    public float ZImpact => Impact * 0.1f;

    public bool WantsTargetOrientation => FireWithTarget?.Exists == true;

    public bool CurrentShotIsDone { get; private set; }

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
                    if (Vector3.Angle(Forward, transform.forward) < 5)
                        instance.transform.rotation = Quaternion.LookRotation(Forward);
                    else
                        instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                    instance.transform.localPosition = Vector3.zero;
                    shot = instance.GetComponent<RailgunShot>();
                    shot.owner = echelon;
                    shot.damage = damage;
                    shot.speedMetersPerSecond = speedMetersPerSecond;
                }
            }
        }
        else
            CurrentShotIsDone = false;


        if (shot)
        {
            shot.canFire = Vector3.Angle(Forward, transform.forward) < 5;

            if (shot.HasFired)
            {
                shot.transform.SetParent(echelon.transform.parent);
            }
            else
            {
                if (Vector3.Angle(Forward, transform.forward) < 5)
                    shot.transform.rotation = Quaternion.LookRotation(Forward);
                else
                    shot.transform.localRotation = Quaternion.identity;
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
