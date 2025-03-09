using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyLevel : PerformanceCaptured_U
{
    private MeshRenderer meshRenderer;
    private Material material;
    public float maxEnergy = 1;
    public float currentEnergy;
    public float currentChange;
    
    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.materials[0];
    }

    // Update is called once per frame
    protected override void P_Update()
    {
        perf.Update(() =>
        {
            material.SetVector("_EnergyLevel", new Vector4(Mathf.Max(maxEnergy, 0.01f), currentEnergy, currentChange));
        });
    }
}
