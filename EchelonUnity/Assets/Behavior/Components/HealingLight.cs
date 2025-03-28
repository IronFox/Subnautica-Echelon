using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingLight : MonoBehaviour
{
    private Material material;
    private Light myLight;
    private MeshRenderer myRenderer;
    public bool isHealing;
    private double t = 0;
    public bool isEnabled = true;
    private float intensity;
    // Start is called before the first frame update
    void Start()
    {
        myRenderer = GetComponent<MeshRenderer>();
        material = myRenderer.materials[0];
        myLight = GetComponentInChildren<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isEnabled)
        {
            myRenderer.enabled = false;
            return;
        }
        if (isHealing)
            intensity += Time.deltaTime;
        else
            intensity -= Time.deltaTime;
        intensity = M.Saturate(intensity);

        t += Time.deltaTime;

        float actualIntensity = (float)(Math.Sin(t*2)*0.2 + 0.6) * intensity;

        material.SetFloat("_HealingVisibility", actualIntensity * 0.5f);
        myRenderer.enabled = actualIntensity > 0;
        if (myLight != null)
        {
            myLight.intensity = actualIntensity;
            myLight.enabled = actualIntensity > 0;
        }
    }
}
