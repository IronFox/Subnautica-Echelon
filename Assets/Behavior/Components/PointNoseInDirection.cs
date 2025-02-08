using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointNoseInDirection : MonoBehaviour
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


    private Vector2 Flat(Vector3 source) => M.FlatNormalized(source);

    public float HorizontalRotationIntent => enabled ? rotX : 0;

    private float SignedMin(float signed, float max)
    {
        return Mathf.Sign(signed) * Mathf.Min(Mathf.Abs(signed), max);
    }


    private float UpAngle(Vector3 vector, Vector2 flatAxis)
    {
        var forward = M.FlatNormal(flatAxis);
        return Vector3.SignedAngle(M.UnFlat(forward), vector, M.UnFlat(flatAxis));
        //=> Vector3.ang
        //return Mathf.Atan2(vector.y, Vector2.Dot(Flat(vector),forward)) * 180f / Mathf.PI;
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        rotX = RotateHorizontal();
        //RotateDirect();
        RotateZ(rb, -rotX/5);
        if (rotateUpDown)
            RotateUpDown();
    }

    //void LateUpdate()
    //{
    //    rb.transform.eulerAngles = new Vector3(rb.transform.eulerAngles.x, rb.transform.eulerAngles.y, -rotX / 10);

    //}

    public static void RotateZ(Rigidbody rb, float targetZ)
    {
        var axis = rb.transform.forward;
        //var correct = -Vector3.Dot(rb.angularVelocity, axis);
        //rb.AddTorque(axis * correct, ForceMode.VelocityChange);



        var delta = targetZ - rb.rotation.eulerAngles.z;
        while (delta < -180)
            delta += 360;
        while (delta > 180)
            delta -= 360;
        float wantTurn = -delta * 5;
        if (Mathf.Abs(wantTurn) < 10)
            wantTurn = 0;
        //SignedMin(delta * horizontalRotationAcceleration, maxHorizontalRotationSpeed);
        float haveTurn = -Vector3.Dot(rb.angularVelocity, axis) * 180 / Mathf.PI;
        float error = wantTurn - haveTurn;
        float accel = error * 10;

        //SignedMin((wantTurn - haveTurn)*10, 10);
        rb.AddTorque(axis * -accel * Time.fixedDeltaTime, ForceMode.Acceleration);

    }

    private void RotateUpDown()
    {
        var axis = -M.UnFlat(M.FlatNormal(Flat(rb.transform.forward)));
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
        float error = wantTurn - haveTurn;
        

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

        float wantTurn = delta * 5;
        if (Mathf.Abs(wantTurn) < 10)
            wantTurn = 0;
        float haveTurn = Vector3.Dot( rb.angularVelocity, axis) * 180 / Mathf.PI;

        float error = wantTurn - haveTurn;
        float accel = error * 10;


        rb.AddTorque(axis * accel * Time.fixedDeltaTime, ForceMode.Acceleration);


    }



    private float RotateHorizontal()
    {
        var upDownAxis = rb.transform.right;

        var normalForward = M.FlatNormal(Flat(upDownAxis));
        var directForward = Flat(rb.transform.forward);
        var correctedForward = (directForward + normalForward) / 2;
        var forward = Mathf.Atan2(correctedForward.y, correctedForward.x) * 180f / Mathf.PI;
        var delta1 = Vector2.SignedAngle(Flat(targetOrientation.forward), directForward);
        //var delta2 = Vector2.SignedAngle(Flat(targetOrientation.forward), normalForward);
        var delta = delta1;
        //if (Mathf.Abs(delta) > Mathf.Abs(delta2))
        //    delta = delta2;
        float wantTurn = delta * 5;
        if (Mathf.Abs(wantTurn) < 10)
            wantTurn = 0;
        //SignedMin(delta * horizontalRotationAcceleration, maxHorizontalRotationSpeed);
        //float haveTurn = -Vector3.Dot(rb.angularVelocity, axis) * 180 / Mathf.PI;
        float haveTurn = rb.angularVelocity.y * 180 / Mathf.PI;
        float error = wantTurn - haveTurn;
        float accel = error * 10;

        //SignedMin((wantTurn - haveTurn)*10, 10);
        rb.AddTorque(0, accel * Time.fixedDeltaTime, 0, ForceMode.Acceleration);

        return wantTurn;

    }
}
