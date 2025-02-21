using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonCameraOrientation : MonoBehaviour, IDirectionSource
{
    public float rightRotationSpeed;
    public float upRotationSpeed;
    public bool isActive;
    private LockedEuler rot;

    public Vector3 Forward => rot.Forward;
    public Vector3 Right => rot.Right;
    public Vector3 Up => rot.Up;

    // Start is called before the first frame update
    void Start()
    {
        
    }



    // Update is called once per frame
    void Update()
    {
        if (!isActive)
        {
            rot = LockedEuler.FromGlobal(transform);
        }
        else
        {
            //Debug.Log(upRotationSpeed);
            rot = rot.RotateBy(upRotationSpeed, rightRotationSpeed, Time.deltaTime);
        }
    }
}
