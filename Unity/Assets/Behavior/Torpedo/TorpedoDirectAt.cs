using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoDirectAt : MonoBehaviour
{
    public Vector3 targetDirection;
    private float maxRotationDegreesPerSecond = 300f;
    private Rigidbody rb;
    private const float DeltaToAccelerationFactor = 10;
    public bool emulateRigidbody = true;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError($"Rigid body adapter not found on torpedo");
        targetDirection = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        try
        {
            var maxRotThisFrame = maxRotationDegreesPerSecond * Time.fixedDeltaTime;
            var error = Vector3.Angle(transform.forward, targetDirection);
            //Debug.Log($"Torpedo.DirectAt: error={error}, maxRotThisFrame={maxRotThisFrame}, timeDelta={Time.fixedDeltaTime}");

            if (emulateRigidbody)
            {
                var q = Quaternion.FromToRotation(transform.forward, targetDirection);
                var actual  = Quaternion.Slerp(Quaternion.identity, q, Mathf.Min(maxRotThisFrame, error*0.06f));
                transform.rotation = actual * transform.rotation;
            }
            else
            {
                //transform.forward = targetDirection;
                //rb.angularVelocity = Vector3.zero;

                var axis = Vector3.Cross(transform.forward, targetDirection).normalized;
                var want = axis * M.DegToRad(maxRotationDegreesPerSecond) * Mathf.Min(error / maxRotThisFrame, 1);
                var delta = (want - rb.angularVelocity);
                //Debug.Log($"Torpedo.DirectAt: terminal, delta={delta}, want={want}, have={rb.angularVelocity}");
                rb.AddTorque(delta, ForceMode.VelocityChange);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"TorpedoDirectAt.FixedUpdate() failed on rb {rb}");
            Debug.LogException(ex, this);
        }
    }
}
