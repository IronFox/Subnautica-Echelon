using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableCutout : MonoBehaviour
{
    private MaterialAccess access;
    // Start is called before the first frame update
    void Start()
    {
        access = ShaderUtil.Access(transform);
        access.SetFloat($"_EnableCutOff", 1f);
        access.SetFloat($"_EnableDitherAlpha", 1f);
    }

    // Update is called once per frame
    void Update()
    {
        //if (access.IsActive)
        //    access.SetFloat($"_EnableCutOff", 1f);

    }
}
