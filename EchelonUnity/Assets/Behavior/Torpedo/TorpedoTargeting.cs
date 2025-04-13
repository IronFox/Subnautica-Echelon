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
        look = GetComponent<TorpedoDirectAt>();
        predictor = GetComponent<TargetPredictor>();
        rb = GetComponent<Rigidbody>();
        drive = GetComponent<TorpedoDrive>();

    }



    // Update is called once per frame
    void Update()
    {
    }


    private Vector3 RelativeSolution()
    {

        var predictionP = predictor.CurentPrediction;
        if (predictionP == null)
        {
            return transform.forward; //maintain current direction
        }

        return M.Intercept(predictionP.Value, new LinearPrediction(rb.velocity, transform.position));
    }


    void FixedUpdate()
    {
        targetError = 0;

        var target = predictor.CurentPrediction;
        if (target == null)
            return;

        float interceptVelocity = drive.TravelVelocity;


        drive.targetDirection = look.targetDirection = M.Intercept(target.Value, transform.position, interceptVelocity, maxLookAheadSeconds);
    }
}
