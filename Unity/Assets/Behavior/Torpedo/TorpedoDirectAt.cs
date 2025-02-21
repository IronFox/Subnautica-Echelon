using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoDirectAt : MonoBehaviour
{
    public Vector3 targetDirection;
    private float maxRotationDegreesPerSecond = 300f;
    private RigidbodyAdapter rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<RigidbodyAdapter>();
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
            if (error < maxRotThisFrame)
            {
                //transform.forward = targetDirection;
                //rb.angularVelocity = Vector3.zero;

                var axis = Vector3.Cross(transform.forward, targetDirection).normalized;
                var want = axis * M.DegToRad(maxRotationDegreesPerSecond) * (error / maxRotThisFrame);
                var delta = want - rb.angularVelocity;
                rb.AddTorque(delta, ForceMode.VelocityChange);
            }
            else
            {
                var axis = Vector3.Cross(transform.forward, targetDirection).normalized;
                var want = axis * M.DegToRad(maxRotationDegreesPerSecond);
                var delta = want - rb.angularVelocity;
                rb.AddTorque(delta, ForceMode.VelocityChange);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"FixedUpdate() failed on rb {rb}");
            Debug.LogException(ex, this);
        }
    }
}
