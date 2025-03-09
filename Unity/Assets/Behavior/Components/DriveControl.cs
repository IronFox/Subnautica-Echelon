using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveControl : PerformanceCaptured_U
{
    public Transform propeller;
    public float maxRPS = 100;
    public ParticleSystem regularParticleSystem;
    public ParticleSystem overdriveParticleSystem;
    public AudioSource regularAudioSource;
    public AudioSource overdriveAudioSource;
    public float thrust;
    public float overdrive;
    
    
    private float emissionSpeed;
    private float emissionRate;

    private Vector3 lastPosition;

    void Start()
    {
        emissionSpeed = regularParticleSystem.main.startSpeedMultiplier;
        emissionRate = regularParticleSystem.emission.rateOverTimeMultiplier;
        lastPosition = regularParticleSystem.transform.position;
    }

    // Update is called once per frame
    protected override void P_Update()
    {
        thrust = Mathf.Clamp(thrust, -1, 1);

        //if (regularAudioSource != null)
        //{
        //    var audioThrust = Mathf.Abs(thrust);
        //    if (audioThrust > 0)
        //    {
        //        regularAudioSource.volume = audioThrust;
        //        regularAudioSource.pitch = 1 + audioThrust;
        //        regularAudioSource.enabled = true;
        //    }
        //    else
        //        regularAudioSource.enabled = false;
        //}

        if (overdrive > 0)
        {
            if (overdriveAudioSource != null)
            {
                //overdriveAudioSource.volume = overdrive;
                //overdriveAudioSource.pitch = 0.5f + overdrive;
                //overdriveAudioSource.enabled = true;
            }
            if (overdriveParticleSystem != null)
            {
                var em = overdriveParticleSystem.emission;
                em.enabled = true;
                var main = overdriveParticleSystem.main;
                main.startSize = overdrive;
                main.startLifetime = 0.2f * overdrive;
            }
        }
        else
        {
            //if (overdriveAudioSource != null)
            //    overdriveAudioSource.enabled = false;
            if (overdriveParticleSystem != null)
            {
                var em = overdriveParticleSystem.emission;
                em.enabled = false;
            }
        }

        {
            var inh = regularParticleSystem.inheritVelocity;
            inh.mode = ParticleSystemInheritVelocityMode.Initial;

            var module = regularParticleSystem.main;
            module.startSpeedMultiplier = emissionSpeed * thrust;

            var em = regularParticleSystem.emission;

            var velocity = regularParticleSystem.transform.position - lastPosition;

            em.enabled = thrust > 0 && Vector3.Dot(velocity, regularParticleSystem.transform.forward) < 0;
            em.rateOverTimeMultiplier = emissionRate * 5 * (M.Abs(thrust) + overdrive);
            propeller.Rotate(0, 0, thrust * maxRPS * Time.deltaTime);

            lastPosition = regularParticleSystem.transform.position;
        }
    }
}
