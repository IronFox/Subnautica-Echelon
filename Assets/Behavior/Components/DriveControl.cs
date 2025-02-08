using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveControl : MonoBehaviour
{
    // Start is called before the first frame update
    
    public Transform propeller;
    public float maxRPS = 100;
    private ParticleSystem ps;

    public float thrust;
    
    private float emissionSpeed;
    private float emissionRate;

    private Vector3 lastPosition;

    void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        emissionSpeed = ps.main.startSpeedMultiplier;
        emissionRate = ps.emission.rateOverTimeMultiplier * 10;
        lastPosition = ps.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        thrust = Mathf.Clamp(thrust, -1,1);
        var inh = ps.inheritVelocity;
        inh.mode = ParticleSystemInheritVelocityMode.Initial;

        var module = ps.main;
        module.startSpeedMultiplier = emissionSpeed * thrust;
        
        var em = ps.emission;
        
        var velocity = ps.transform.position - lastPosition;

        em.enabled = thrust > 0 && Vector3.Dot(velocity, ps.transform.forward) < 0;
        em.rateOverTimeMultiplier = emissionRate * thrust * thrust;
        propeller.Rotate(0, 0, thrust * maxRPS * Time.deltaTime);

        lastPosition = ps.transform.position;
    }
}
