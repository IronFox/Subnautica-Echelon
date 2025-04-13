using UnityEngine;


public class TorpedoTargeting : MonoBehaviour
{
    private TargetPredictor predictor;
    private Rigidbody rb;
    private TorpedoDirectAt look;
    private TorpedoDrive drive;

    private float maxLookAheadSeconds = 10;
    public float minLookAheadDistance = 1;
    public float targetError = 0;




    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("TorpedoTargeting: start()");
        look = GetComponent<TorpedoDirectAt>();
        predictor = GetComponent<TargetPredictor>();
        rb = GetComponent<Rigidbody>();
        drive = GetComponent<TorpedoDrive>();

    }



    // Update is called once per frame
    void Update()
    {
        //   Debug.Log("Update");

    }


    private Vector3 RelativeSolution()
    {

        var predictionP = predictor.CurentPrediction;
        if (predictionP == null)
        {
            //Debug.Log("Got no prediction");
            return transform.forward; //maintain current direction
        }

        return M.Intercept(predictionP.Value, new LinearPrediction(rb.velocity, transform.position));
    }


    void FixedUpdate()
    {
        targetError = 0;

        //Debug.Log("Predicting");

        var target = predictor.CurentPrediction;
        if (target == null)
            return;

        float interceptVelocity = drive.TravelVelocity;

        var rp = target.Value.Position - transform.position;

        var solution = M.SolveQuadraticEquation(
            a: M.Sqr(target.Value.Velocity) - M.Sqr(interceptVelocity),
            b: 2 * M.Dot(rp, target.Value.Velocity),
            c: M.Sqr(rp));

        var t = Mathf.Min(maxLookAheadSeconds, solution.SmallestNonNegative ?? 0);

        //Debug.Log($"Lookahead @{t}");

        var relLookAheadTarget = rp + target.Value.Velocity * t;



        //Debug.Log(relTargetPosition + "+" + relTargetVelocity + "*" + lookAheadSeconds + "=" + relLookAheadTarget);

        Debug.DrawLine(transform.position, transform.position + relLookAheadTarget, Color.red);


        drive.targetDirection = look.targetDirection = relLookAheadTarget.normalized;


        ////var directAt = RelativeSolution();// relLookAheadTarget.normalized;
        //if (t.HasValue)
        //{
        //    var wantSpeedPerSecond = relLookAheadTarget / t.Value;
        //    var haveSpeedPerSecond = rb.velocity;
        //    var error = wantSpeedPerSecond - haveSpeedPerSecond;

        //    var directAt = (error.magnitude > 1 ? error : wantSpeedPerSecond).normalized;


        //drive.throttle = M.Max(0, M.Dot(look.targetDirection, rb.velocity.normalized))*0.8f + 0.2f;// * 0.5f;

        //}
        //Debug.Log(mag + " => "+directAt);
        //wantDirection = LockedEuler.FromForward(directAt, TransformLocality.Global);
        //Debug.Log(directAt);
    }
}
