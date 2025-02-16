using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    //public GameObject fireRingPrefab;
    //public GameObject pressureRingPrefab;
    //private GameObject 
    //    fireRing0,
    //    fireRing1,
    //    fireRing2,
    //    pressureRing0,
    //    pressureRing1
        
    //    ;

    private Light[] lights = Array.Empty<Light>();

    private float time;

    // Start is called before the first frame update
    void Start()
    {
        lights = GetComponentsInChildren<Light>();
        AudioPatcher.PatchAll(transform);
        //fireRing0 = Instantiate(fireRingPrefab, transform);
        //fireRing1 = Instantiate(fireRingPrefab, transform);
        //fireRing2 = Instantiate(fireRingPrefab, transform);
        //fireRing1.transform.localEulerAngles = M.V3(Random.Range(-180f,180f),Random.Range(-90f,90f),0);
        //pressureRing0 = Instantiate(pressureRingPrefab, transform);   
        //pressureRing1 = Instantiate(pressureRingPrefab, transform);
        //pressureRing1.transform.localEulerAngles = fireRing1.transform.localEulerAngles;

        //HierarchyAnalyzer hierarchyAnalyzer = new HierarchyAnalyzer();
        //hierarchyAnalyzer.LogToJson(transform, @"C:\temp\logs\explosion.json");
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
