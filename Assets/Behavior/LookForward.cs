using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LookForward : MonoBehaviour
{
    public Transform targetOrientation;

    private Rigidbody rb;

    float rotX = 0;

    public bool rotateUpDown = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    private Vector2 Flat(Vector3 source) => new Vector2(source.x, source.z).normalized;

    public float HorizontalRotationIntent => rotX;

    private Vector3 Unflat(Vector2 flat) => new Vector3(flat.x, 0, flat.y);
    private float SignedMin(float signed, float max)
    {
        return Mathf.Sign(signed) * Mathf.Min(Mathf.Abs(signed), max);
    }

    private Vector2 FlatNormal(Vector2 flatAxis)
    {
        return new Vector2(-flatAxis.y, flatAxis.x);
    }

    private float UpAngle(Vector3 vector, Vector2 flatAxis)
    {
        var forward = FlatNormal(flatAxis);
        return Vector3.SignedAngle(Unflat(forward), vector, Unflat(flatAxis));
        //=> Vector3.ang
        //return Mathf.Atan2(vector.y, Vector2.Dot(Flat(vector),forward)) * 180f / Mathf.PI;
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        rotX = RotateHorizontal();
        //RotateDirect();
        RotateZ(-rotX/10);
        if (rotateUpDown)
            RotateUpDown();
    }

    //void LateUpdate()
    //{
    //    rb.transform.eulerAngles = new Vector3(rb.transform.eulerAngles.x, rb.transform.eulerAngles.y, -rotX / 10);

    //}

    private void RotateZ(float targetZ)
    {
        var axis = rb.transform.forward;
        //var correct = -Vector3.Dot(rb.angularVelocity, axis);
        //rb.AddTorque(axis * correct, ForceMode.VelocityChange);



        //Debug.Log("@: " + rb.rotation.eulerAngles.z);
        var delta = targetZ - rb.rotation.eulerAngles.z;
        while (delta < -180)
            delta += 360;
        while (delta > 180)
            delta -= 360;
        //Debug.Log("delta: " + delta);
        float wantTurn = -delta * 5;
        if (Mathf.Abs(wantTurn) < 10)
            wantTurn = 0;
        //SignedMin(delta * horizontalRotationAcceleration, maxHorizontalRotationSpeed);
        float haveTurn = -Vector3.Dot(rb.angularVelocity, axis) * 180 / Mathf.PI;
        //Debug.Log("want: " + wantTurn);
        //Debug.Log("have: " + haveTurn);
        float error = wantTurn - haveTurn;
        float accel = error * 10;

        //SignedMin((wantTurn - haveTurn)*10, 10);
        rb.AddTorque(axis * -accel * Time.fixedDeltaTime, ForceMode.Acceleration);

    }

    private void RotateUpDown()
    {
        var axis = -Unflat(FlatNormal(Flat(rb.transform.forward)));
//            Unflat(Flat(rb.transform.right));
            //Vector3.Cross(Vector3.up, rb.transform.forward);
        float have = UpAngle(rb.transform.forward, Flat(axis));
        float want = UpAngle(targetOrientation.forward, Flat(targetOrientation.right));

        var delta = Mathf.DeltaAngle(have, want);
        float wantTurn = delta * 5;
        if (Mathf.Abs(wantTurn) < 10)
            wantTurn = 0;
        //SignedMin(delta * horizontalRotationAcceleration, maxHorizontalRotationSpeed);
        float haveTurn = Vector3.Dot( rb.angularVelocity, axis) * 180 / Mathf.PI;
        //Debug.Log("want: " + wantTurn);
        //Debug.Log("have: " + haveTurn);
        float error = wantTurn - haveTurn;

        //if (have < -80f)
        //{
        //    rb.AddTorque(axis * -haveTurn, ForceMode.VelocityChange);
        //    rb.transform.eulerAngles = new Vector3(-79, rb.transform.eulerAngles.y, rb.transform.eulerAngles.z);
        //    Debug.Log("threshold");
        //    return;

        //}
        

        float accel = error * 10;

        //if (rb.transform.eulerAngles.x > 45f && haveTurn < 0)
          //  rb.AddTorque(axis * -haveTurn * Time.fixedDeltaTime, ForceMode.VelocityChange);
        //else
        //SignedMin((wantTurn - haveTurn)*10, 10);
        rb.AddTorque(axis * accel * Time.fixedDeltaTime, ForceMode.Acceleration);

    }

    private void RotateDirect()
    {
        var haveForward = rb.transform.forward;
        var wantForward = targetOrientation.forward;
        var axis = Vector3.Cross( haveForward, wantForward);
        var len = axis.magnitude;
        if (len == 0)
        {
            return;
        }
        axis /= len;

        var delta = Vector3.Angle(haveForward, wantForward);
        Debug.Log("delta: " + delta);

        float wantTurn = delta * 5;
        if (Mathf.Abs(wantTurn) < 10)
            wantTurn = 0;
        float haveTurn = Vector3.Dot( rb.angularVelocity, axis) * 180 / Mathf.PI;

        Debug.Log("want: " + wantTurn);
        Debug.Log("have: " + haveTurn);

        float error = wantTurn - haveTurn;
        float accel = error * 10;


        rb.AddTorque(axis * accel * Time.fixedDeltaTime, ForceMode.Acceleration);


    }



    private float RotateHorizontal()
    {
        var upDownAxis = rb.transform.right;

        var normalForward = FlatNormal(Flat(upDownAxis));
        var directForward = Flat(rb.transform.forward);
        var correctedForward = (directForward + normalForward) / 2;
        var forward = Mathf.Atan2(correctedForward.y, correctedForward.x) * 180f / Mathf.PI;
        Debug.Log("Forward: " + forward);
        var delta1 = Vector2.SignedAngle(Flat(targetOrientation.forward), directForward);
        //var delta2 = Vector2.SignedAngle(Flat(targetOrientation.forward), normalForward);
        var delta = delta1;
        //if (Mathf.Abs(delta) > Mathf.Abs(delta2))
        //    delta = delta2;
        //Debug.Log("delta: " + delta);
        float wantTurn = delta * 5;
        if (Mathf.Abs(wantTurn) < 10)
            wantTurn = 0;
        //SignedMin(delta * horizontalRotationAcceleration, maxHorizontalRotationSpeed);
        //float haveTurn = -Vector3.Dot(rb.angularVelocity, axis) * 180 / Mathf.PI;
        float haveTurn = rb.angularVelocity.y * 180 / Mathf.PI;
        //Debug.Log("want: " + wantTurn);
        //Debug.Log("have: " + haveTurn);
        float error = wantTurn - haveTurn;
        float accel = error * 10;

        //SignedMin((wantTurn - haveTurn)*10, 10);
        rb.AddTorque(0, accel * Time.fixedDeltaTime, 0, ForceMode.Acceleration);

        return wantTurn;

    }
}
