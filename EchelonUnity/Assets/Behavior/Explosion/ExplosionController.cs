using System;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{

    private Light[] lights = Array.Empty<Light>();
    private SoundAdapter sound;
    private float time;

    //public static float explosionDamage = 1500;


    public float explosionRadius = 15f;
    public float explosionDamage = 1500f;
    public CameraShake cameraShake;


    // Start is called before the first frame update
    void Start()
    {
        lights = GetComponentsInChildren<Light>();
        sound = GetComponent<SoundAdapter>();

        sound.minDistance = explosionRadius / 2;

        var exp = transform.GetChild(0);
        exp.localScale = M.V3(explosionRadius * 2f / 15f);
        if (cameraShake)
            cameraShake.SignalExplosionStart(transform, explosionRadius);

        foreach (var env in GetCurrentEnvironment())
        {
            var t = TargetAdapter.ResolveTarget(env.gameObject, env);
            if (t != null && t.IsAlive)
            {
                float distance = M.Distance(transform.position, t.GameObject.transform.position);
                float dmg = explosionDamage / (1f + distance * 0.1f);
                t.DealDamage(transform.position, dmg, gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        foreach (Light light in lights)
        {
            light.intensity = 1f - M.Smoothstep(1.6f, 1.8f, time);
        }

        if (time > 3.5f)
            Destroy(gameObject);
    }


    private IEnumerable<Rigidbody> GetCurrentEnvironment()
    {
        var others = Physics.OverlapSphere(transform.position, explosionRadius);
        Dictionary<int, Rigidbody> bodies = new Dictionary<int, Rigidbody>();
        foreach (var other in others)
        {
            var rb = other.attachedRigidbody;
            if (rb == null)
                continue;
            bodies[rb.GetInstanceID()] = rb;
        }
        return bodies.Values;
    }

    void FixedUpdate()
    {
        foreach (var rb in GetCurrentEnvironment())
        {
            rb.AddExplosionForce(1000 / (1f + time), transform.position, 20, 0);
        }
    }
}
