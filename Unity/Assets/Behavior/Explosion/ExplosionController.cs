using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ExplosionController : PerformanceCaptured_UF
{

    private Light[] lights = Array.Empty<Light>();

    private float time;

    //public static float explosionDamage = 1500;


    public float explosionRadius = 15f;
    public float explosionDamage = 1500f;

    

    // Start is called before the first frame update
    void Start()
    {
        lights = GetComponentsInChildren<Light>();



        var exp = transform.GetChild(0);
        exp.localScale = M.V3(explosionRadius * 2f / 15f);


        foreach (var env in GetCurrentEnvironment())
        {
            var t = TargetAdapter.ResolveTarget(env.gameObject, env);
            if (t != null && t.IsAlive)
            {
                float distance = M.Distance(transform.position,t.GameObject.transform.position); 
                float dmg = explosionDamage / (1f + distance * 0.1f);
                //ConsoleControl.Write($"Dealing {dmg} damage to {t} at health {t.CurrentHealth} and distance {distance}");
                t.DealDamage(transform.position, dmg, gameObject);
                //ConsoleControl.Write($"Health now at {t.CurrentHealth}");
            }
        }
    }

    // Update is called once per frame
    protected override void P_Update()
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

    protected override void P_FixedUpdate()
    {
        foreach (var rb in GetCurrentEnvironment())
        {
            rb.AddExplosionForce(1000 / (1f + time), transform.position, 20, 0);
        }
    }
}
