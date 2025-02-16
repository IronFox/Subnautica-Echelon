using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{

    private Light[] lights = Array.Empty<Light>();

    private float time;

    // Start is called before the first frame update
    void Start()
    {
        lights = GetComponentsInChildren<Light>();
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
}
