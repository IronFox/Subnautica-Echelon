using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoDirectAt : MonoBehaviour
{
    public Vector3 targetDirection;
    private float maxRotationDegreesPerSecond = 50f;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        var maxRotThisFrame = maxRotationDegreesPerSecond * Time.fixedDeltaTime;
        var error = Vector3.Angle(transform.forward, targetDirection);
        if (error < maxRotThisFrame)
        {
            transform.forward = targetDirection;
        }
        else
        {
            var axis = Vector3.Cross(transform.forward, targetDirection);
            rb.AddTorque(axis * M.DegToRad(maxRotationDegreesPerSecond), ForceMode.VelocityChange);
        }
    }
}
