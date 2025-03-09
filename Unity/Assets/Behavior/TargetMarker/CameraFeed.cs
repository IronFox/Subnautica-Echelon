using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFeed : PerformanceCaptured_U
{
    private Material material;
    private MeshRenderer _renderer;
    private MeshFilter filter;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        filter = GetComponent<MeshFilter>();

        material = _renderer.materials[0];
    }

    protected override void P_Update()
    {
        float scale = M.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        filter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one*scale);
        material.SetVector("_ObjCenter", transform.position);
        var camera = CameraUtil.GetTransform(nameof(CameraFeed));
        if (camera != null)
        {
            material.SetVector("_CameraX", camera.right);
            material.SetVector("_CameraY", camera.up);
        }
        material.SetFloat("_Scale", scale);
 
    }
}
