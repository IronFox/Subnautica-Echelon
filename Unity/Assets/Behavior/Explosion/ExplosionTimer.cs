using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionTimer : PerformanceCaptured_U
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
    protected override void P_Update()
    {
        material.SetFloat("_Seconds", (float)(DateTime.Now - started).TotalSeconds);
    }
}
