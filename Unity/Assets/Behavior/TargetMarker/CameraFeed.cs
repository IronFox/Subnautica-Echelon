using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraFeed : MonoBehaviour
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
    // Update is called once per frame
    void Update()
    {
        float scale = M.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        filter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one*scale);
        material.SetVector("_ObjCenter", transform.position);
        material.SetVector("_CameraCenter", Camera.main.transform.position);
        material.SetVector("_CameraX", Camera.main.transform.right);
        material.SetVector("_CameraY", Camera.main.transform.up);
        material.SetFloat("_Scale", scale);
 
    }
}
