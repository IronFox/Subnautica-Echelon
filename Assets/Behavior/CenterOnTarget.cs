using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOnTarget : MonoBehaviour
{
    public Transform target;
    public float radius = 20;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void LateUpdate()
    {
        if (target != null)
        {
            var delta = (target.position - transform.position).normalized * radius;
            transform.position = target.position - delta;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
