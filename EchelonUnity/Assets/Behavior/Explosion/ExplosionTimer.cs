using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionTimer : MonoBehaviour
{
    private DateTime started;
    private Material material;
    // Start is called before the first frame update
    void Start()
    {
        material = MaterialAccess.From(transform).Material;
        started = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        material.SetFloat("_Seconds", (float)(DateTime.Now - started).TotalSeconds);
    }
}
