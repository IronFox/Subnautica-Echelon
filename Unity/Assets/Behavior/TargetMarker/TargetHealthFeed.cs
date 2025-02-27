using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHealthFeed : MonoBehaviour
{
    // Start is called before the first frame update
    public TargetAdapter target;
    private Material material;
    void Start()
    {
        material = GetComponent<MeshRenderer>().materials[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
            material.SetVector("_Health", M.V3(target.CurrentHealth, target.MaxHealth,1));
        else
            material.SetVector("_Health", M.V3(0));
    }
}
