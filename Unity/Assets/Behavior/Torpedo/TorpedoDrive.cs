using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoDrive : MonoBehaviour
{
    private float acceleration = 500;
    private float travelVelocity = 35;
    private TurnPropeller turn;
    private Rigidbody rb;
    public float currentVelocity;
    public float dragCompensated;
    public float finalAcceleration;
    //private float errorCorrection = 1f;
    public float TravelVelocity => travelVelocity;

    // Start is called before the first frame update
    void Start()
    {
        turn = GetComponent<TurnPropeller>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
    }


    void FixedUpdate()
    {
        if (!ActorAdapter.IsOutOfWater(gameObject, rb.position))
        {

            var forwardVelocity = currentVelocity = M.Dot(rb.velocity, transform.forward);
            var sideVelocity = rb.velocity - transform.forward * forwardVelocity;

            //dragCompensated = travelVelocity * 2f;

            var error = travelVelocity - forwardVelocity;
            dragCompensated = DragCompensation
                .For(travelVelocity)
                .Update(error, Time.fixedDeltaTime);

            finalAcceleration = Mathf.Min(acceleration, dragCompensated);
            rb.AddRelativeForce(0, 0, finalAcceleration, ForceMode.Acceleration);

            turn.speedScale = 0.1f + 0.9f * Mathf.Max(0f, finalAcceleration / acceleration);
        }
    }
}
