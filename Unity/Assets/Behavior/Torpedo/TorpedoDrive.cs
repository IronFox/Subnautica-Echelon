using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoDrive : PerformanceCaptured_F
{
    private float acceleration = 1000;
    //private float travelVelocity = 35;
    private TurnPropeller turn;
    private Rigidbody rb;
    public Rigidbody origin;
    public float currentVelocity;
    public float dragCompensated;
    public float finalAcceleration;
    public bool triggerActive = true;
    public Vector3 targetDirection;
    //private float errorCorrection = 1f;
    public readonly float TravelVelocity = 45;
    //public float throttle = 1;
    

    // Start is called before the first frame update
    void Start()
    {
        turn = GetComponent<TurnPropeller>();
        rb = GetComponent<Rigidbody>();
        targetDirection = transform.forward;
    }



    protected override void P_FixedUpdate()
    {
        if (!ActorAdapter.IsOutOfWater(gameObject, rb.position))
        {
            if (triggerActive)
            {
                rb.AddRelativeForce(0, 0, acceleration*0.5f, ForceMode.Acceleration);
                return;
            }





            var forwardVelocity = currentVelocity = M.Dot(rb.velocity, targetDirection);
            var sideVelocity = rb.velocity - targetDirection * forwardVelocity;

            //dragCompensated = travelVelocity * 2f;



            var error = TravelVelocity - forwardVelocity;
            dragCompensated = error > 0 ? error * 15 : error;

                //DragCompensation
                //.For(TravelVelocity)
                //.Update(error, Time.fixedDeltaTime);

            finalAcceleration = Mathf.Min(acceleration, dragCompensated);
            rb.AddForce(targetDirection * finalAcceleration, ForceMode.Acceleration);
            rb.AddForce(-sideVelocity, ForceMode.VelocityChange);
            turn.speedScale = 0.1f + 0.9f * Mathf.Max(0f, finalAcceleration / acceleration);
        }
    }
}
