using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEmission : MonoBehaviour
{
    public Texture emissionMap;
    // Start is called before the first frame update
    void Start()
    {
        var access = MaterialAccess.From(transform);
        access.SetTexture("_Illum", emissionMap);
        access.SetTexture("_EmissiveTex", emissionMap);
        access.SetTexture("_Lightmap", emissionMap);
        access.SetTexture("_GlowMask", emissionMap);
        access.SetTexture("_SIGMap", emissionMap);
        access.SetFloat("_EnableGlow", 1f);
        access.SetFloat("_EnableLightmap", 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
