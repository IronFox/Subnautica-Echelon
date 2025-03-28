using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSpecTex : MonoBehaviour
{
    public Texture texture;
    public int materialIndex;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var access = ShaderUtil.Access(transform, materialIndex);
        access.SetTexture("_SpecTex", texture);

    }
}
