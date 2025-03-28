using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallOrientation : MonoBehaviour, IDirectionSource
{
    private Rigidbody rb;
    private LockedEuler rot;

    // Start is called before the first frame update
    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();

    }

    public Vector3 debugForward;
    private float lastVelocity;

    public Vector3 Forward => rot.Forward;
    public Vector3 Right => rot.Right;
    public Vector3 Up => rot.Up;


    // Update is called once per frame
    void Update()
    {
        var dir = rb.velocity;
        var mag = lastVelocity = dir.magnitude;
        if (mag < 0.5f)
            dir = transform.forward;
        else
            dir /= mag;

        rot = LockedEuler.FromForward(dir,TransformLocality.Global);

        debugForward = Forward;
    }

    public float Impact => 1f - 1f / (1f + 0.1f * lastVelocity);
        //M.Smoothstep(3, 10, lastVelocity);
    public float ZImpact => Impact * 0.1f;
}
