using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.VR;
using UnityEngine;

public class TorpedoTargeting : MonoBehaviour
{
    private TargetPredictor predictor;
    private Rigidbody rb;
    private TorpedoDirectAt look;

    public float maxLookAheadSeconds = 10;
    public float minLookAheadDistance = 1;
    public float targetError = 0;



    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("TorpedoTargeting: start()");
        look = GetComponent<TorpedoDirectAt>();
        predictor = GetComponent<TargetPredictor>();
        rb = GetComponent<Rigidbody>();
        

    }



    // Update is called once per frame
    void Update()
    {
     //   Debug.Log("Update");

    }


    void FixedUpdate()
    {
        targetError = 0;

       // Debug.Log("Predicting");

        var predictionP = predictor.CurentPrediction;
        if (predictionP == null)
        {
            //Debug.Log("Got no prediction");
            return; //maintain current direction
        }
        var prediction = predictionP.Value;

        var currentPosition = transform.position;
        var currentTarget = prediction.Position;


        var absTargetVelocity = prediction.Velocity;

        var absTargetSpeed = absTargetVelocity.magnitude;
        var absVelocity = rb.velocity;


        var relVelocity = absVelocity - absTargetVelocity;
        var relTargetVelocity = -relVelocity;
        var relTargetPosition = currentTarget - currentPosition;


        var directionToTarget = relTargetPosition;
        var distance = directionToTarget.magnitude;
        directionToTarget /= distance;
        //if (distance < detonationProximity)
        //{
        //    Detonate();
        //    return;
        //}

        var lookAheadSeconds = maxLookAheadSeconds;
        //var targetVelocityDelta = targetVelocity - (Vector2)m_Body.velocity;
        var approachVelocity = M.Dot(relVelocity, directionToTarget);
        if (approachVelocity > 0)
            lookAheadSeconds = Mathf.Min(lookAheadSeconds, distance / approachVelocity);

        var relLookAheadTarget = relTargetPosition + relTargetVelocity * lookAheadSeconds;


        var lookAheadDistance = relLookAheadTarget.magnitude;

        if (lookAheadDistance < minLookAheadDistance)
        {
            //Debug.Log("Retract target projection: "+relTargetVelocity+" => "+lookAheadDistance);
            relLookAheadTarget = relTargetPosition.normalized * minLookAheadDistance * 1.1f;
        }
        //float apply = Mathf.Min(1.0f, (lookAheadDistance - detonationProximity / 2) * 0.5f);







        //Debug.Log(relTargetPosition + "+" + relTargetVelocity + "*" + lookAheadSeconds + "=" + relLookAheadTarget);

        Debug.DrawLine(currentPosition, currentPosition + relLookAheadTarget, Color.red);

        var directAt = relLookAheadTarget.normalized;
        look.targetDirection = directAt;
        //wantDirection = LockedEuler.FromForward(directAt, TransformLocality.Global);
        //Debug.Log(directAt);
        targetError = 1f - M.Dot(directAt, transform.forward);
    }
}
