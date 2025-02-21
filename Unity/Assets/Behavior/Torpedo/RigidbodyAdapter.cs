using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyAdapter : MonoBehaviour
{
    public static Func<GameObject, float, Rigidbody> MakeRigidbody { get; set; } = (go, mass) =>
    {
        ConsoleControl.Write($"Creating default rigidbody for {go} with mass {mass}");
        try
        {
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.drag = 10;
            rb.angularDrag = 10;
            ConsoleControl.Write("Rigidbody created: " + rb);
            return rb;
        }
        catch (Exception ex)
        {
            ConsoleControl.WriteException($"RigidbodyAdapter.MakeRigidbody() (default)", ex);
            throw;
        }
    };

    private Rigidbody _rigidbody;
    private Rigidbody body
    {
        get
        {
            if (_rigidbody == null)
                _rigidbody = MakeRigidbody(gameObject, mass);
            if (_rigidbody == null)
                throw new InvalidOperationException($"Rigid body remains null after MakeRigidbody()");
            return _rigidbody;
        }
    }
    public float mass = 100f;

    internal bool isKinematic
    {
        get => body.isKinematic;
        set => body.isKinematic = value;
    }
    internal Vector3 angularVelocity
    {
        get => body.angularVelocity;
        set => body.angularVelocity = value;
    }

    public Vector3 velocity
    {
        get => body.velocity;
        set => body.velocity = value;
    }

    public void AddRelativeForce(float x, float y, float z, ForceMode fm)
    {
        body.AddRelativeForce(x, y, z, fm);
    }

    public void AddTorque(Vector3 torque, ForceMode fm)
    {
        body.AddTorque(torque, fm);
    }

    // Start is called before the first frame update
    void Start()
    {}

    // Update is called once per frame
    void Update()
    {
        
    }
}



