using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableCutout : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var renderer = GetComponent<MeshRenderer>();

        if (renderer != null && renderer.materials.Length == 1)
        {
            renderer.materials[0].SetInt($"_EnableCutOff",1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
