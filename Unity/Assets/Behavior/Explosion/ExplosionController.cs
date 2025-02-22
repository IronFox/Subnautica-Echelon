using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ExplosionController : MonoBehaviour
{

    private Light[] lights = Array.Empty<Light>();

    private float time;

    public const float ExplosionRadius = 15;

    // Start is called before the first frame update
    void Start()
    {
        lights = GetComponentsInChildren<Light>();
        ExplosionAdapter.HandleExplosion((gameObject, transform.position, 50, 100));

        foreach (var env in GetCurrentEnvironment())
        {
            var t = TargetAdapter.ResolveTarget(env.gameObject, env);
            if (t != null && t.IsAlive)
            {
                float distance = M.Distance(transform.position,t.GameObject.transform.position); 
                float dmg = 2000 / (1f + distance * 0.1f);
                ConsoleControl.Write($"Dealing {dmg} damage to {t} at health {t.CurrentHealth} and distance {distance}");
                t.DealDamage(transform.position, dmg, gameObject);
                ConsoleControl.Write($"Health now at {t.CurrentHealth}");
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
        var others = Physics.OverlapSphere(transform.position, ExplosionRadius);
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
