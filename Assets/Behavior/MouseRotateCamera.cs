using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseRotateCamera : MonoBehaviour
{
    public float lookSpeed = 3;
    private Vector2 rotation = Vector2.zero;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        transform.eulerAngles = new Vector2(
            Mathf.Clamp(rotation.x * lookSpeed, -85f, 85f),
            rotation.y * lookSpeed) ;
    }
}
