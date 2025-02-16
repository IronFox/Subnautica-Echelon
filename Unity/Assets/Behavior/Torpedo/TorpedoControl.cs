using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TorpedoControl : MonoBehaviour, IDirectionSource
{
    private TargetPredictor predictor;
    private Rigidbody rb;
    private TurnPropeller tp;
    private PointNoseInDirection look;
    private ProximityDetector detector;

    public Collider normalCollider;
    
    private Vector3 relTargetPosition;
    private Vector3 relTargetVelocity;
    private Vector3 relLookAheadTarget;

    //public float detonationProximity = 1;
    
    private float lookAheadSeconds;
    private float currentDeltaVelocity;
    private float correctionDistance;
    private float travelCorrectionTime;
    private float travelMagnitude;
    private float currentVelocity;
    private float thrust;

    public float acceleration = 100;
    public float maxLookAheadSeconds = 10;
    public float minLookAheadDistance = 1;
    public float travelVelocity = 100;

    public Rigidbody doNotCollideWith;


    private LockedEuler wantDirection;

    public Vector3 Forward => wantDirection.Forward;

    public Vector3 Right => wantDirection.Right;

    public Vector3 Up => wantDirection.Up;


    // Start is called before the first frame update
    void Start()
    {
        predictor = GetComponent<TargetPredictor>();
        rb = GetComponent<Rigidbody>();
        tp = GetComponent<TurnPropeller>();
        look = GetComponent<PointNoseInDirection>();
        detector = GetComponentInChildren<ProximityDetector>();
        look.targetOrientation = this;
        detector.doNotCollideWith = doNotCollideWith;
    }

    // Update is called once per frame
    void Update()
    {
        detector.doNotCollideWith = doNotCollideWith;

        if (normalCollider.isTrigger && !detector.IsIntersectingWithExclusion)
        {
            ConsoleControl.Write($"Exited exlusion intersection. Restoring collider");
            normalCollider.isTrigger = false;
        }

    }


    void FixedUpdate()
    {
        if (detector.IsIntersectingWithExclusion)
            return;
        //if (!IsActive)
        //{
        //    SetThrusterEffect(0);
        //    return;
        //}

        var predictionP = predictor.CurentPrediction;
        if (predictionP == null)
            return;
        var prediction = predictionP.Value;

        var currentPosition = transform.position;
        var currentTarget = prediction.Position;


        var absTargetVelocity = prediction.Velocity;

        var absTargetSpeed = absTargetVelocity.magnitude;
        var absVelocity = rb.velocity;


        var relVelocity = absVelocity - absTargetVelocity;
        relTargetVelocity = -relVelocity;
        relTargetPosition = currentTarget - currentPosition;


        var directionToTarget = relTargetPosition;
        var distance = directionToTarget.magnitude;
        directionToTarget /= distance;
        //if (distance < detonationProximity)
        //{
        //    Detonate();
        //    return;
        //}

        lookAheadSeconds = maxLookAheadSeconds;
        //var targetVelocityDelta = targetVelocity - (Vector2)m_Body.velocity;
        var approachVelocity = M.Dot(relVelocity, directionToTarget);
        if (approachVelocity > 0)
            lookAheadSeconds = Mathf.Min(lookAheadSeconds, distance / approachVelocity);

        relLookAheadTarget = relTargetPosition + relTargetVelocity * lookAheadSeconds;


        var lookAheadDistance = relLookAheadTarget.magnitude;

        if (lookAheadDistance < minLookAheadDistance)
        {
            //Debug.Log("Retract target projection: "+relTargetVelocity+" => "+lookAheadDistance);
            relLookAheadTarget = relTargetPosition.normalized * minLookAheadDistance * 1.1f;
        }
        //float apply = Mathf.Min(1.0f, (lookAheadDistance - detonationProximity / 2) * 0.5f);



        //Debug.Log(relTargetPosition + "+" + relTargetVelocity + "*" + lookAheadSeconds + "=" + relLookAheadTarget);

        Debug.DrawLine(currentPosition, currentPosition + relLookAheadTarget, Color.red);


        currentDeltaVelocity = relTargetVelocity.magnitude;

        Vector3 accelerate = Vector3.zero;

        /*
		if (approachVelocity > 0)
		{
			var trajectory = -targetVelocityDelta;

			var interceptT = (M.Dot(lookAheadTarget, trajectory) - M.Dot(currentPosition, trajectory)) / M.Sqr(trajectory);
			var intercept = currentPosition + trajectory * interceptT;

			var courseCorrection = lookAheadTarget - intercept;
			var correctionDistance = courseCorrection.magnitude;
			var activationDistance = detonationProximity / 4;
			if (correctionDistance > activationDistance)
			{

				var interceptCorrectionTime = correctionDistance / this.acceleration;
				Debug.Log("inercept in " + interceptT + " error " + correctionDistance + " ct=" + interceptCorrectionTime);
				var interceptMagnitude = Mathf.Min(interceptCorrectionTime, 0.1f) * 10 * acceleration;



				accelerate += interceptMagnitude * courseCorrection.normalized 
					* Math.Min(1.0f, approachVelocity/10)
					* Math.Min(1.0f, (correctionDistance - activationDistance)/100)
					;
			}
		}
		*/
        //if (lookAheadDistance > detonationProximity/2)
        {
            //var lookAheadDistance = relLookAheadTarget.magnitude;
            //float apply = Mathf.Min(1.0f, (lookAheadDistance - detonationProximity / 2) * 0.5f);

            var dir = relLookAheadTarget.normalized;// Vector2.Lerp(relTargetPosition.normalized, relLookAheadTarget.normalized, apply);

            float remaining = acceleration - accelerate.magnitude;

            //var courseCorrection = (lookAheadTarget - currentPosition).normalized * travelVelocity - (Vector2)m_Body.velocity;
            var courseCorrection = dir * travelVelocity - relVelocity;
            correctionDistance = courseCorrection.magnitude;
            travelCorrectionTime = correctionDistance / this.acceleration;
            travelMagnitude = Mathf.Min(travelCorrectionTime, 0.25f) * 4 * remaining;
            accelerate += travelMagnitude * courseCorrection.normalized;
        }

        currentVelocity = rb.velocity.magnitude;

        var accelerationMagnitude = accelerate.magnitude;
        thrust = accelerationMagnitude / acceleration;
        var applyDir = M.Smoothstep(0.1f, 0.5f, thrust);

        var accelMag = accelerate.magnitude;
        wantDirection = LockedEuler.FromForward(relLookAheadTarget.normalized, TransformLocality.Global);
        rb.AddForce(accelerate, ForceMode.Acceleration);

        tp.speedScale = M.Saturate(Mathf.Max(M.Dot(accelerate, transform.forward), absVelocity.magnitude / 10f));
    }

}
