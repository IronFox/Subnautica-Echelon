using System;
using UnityEngine;

public class ExplosionTimer : MonoBehaviour
{
    private DateTime started;
    private Material material;
    private Vector3 scale;
    public float timeSpeed = 6;
    private float t;
    // Start is called before the first frame update
    void Start()
    {
        material = MaterialAccess.From(transform).Material;
        started = DateTime.Now;
        scale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        material.SetFloat("_Seconds", t * timeSpeed);
        transform.localScale = scale * t;
    }
}
