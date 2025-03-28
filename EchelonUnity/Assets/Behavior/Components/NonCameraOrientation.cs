using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonCameraOrientation : MonoBehaviour, IDirectionSource
{
    public float rightRotationSpeed;
    public float upRotationSpeed;
    public bool isActive;
    public bool outOfWater;
    private LockedEuler rot;

    public Vector3 Forward => rot.Forward;
    public Vector3 Right => rot.Right;
    public Vector3 Up => rot.Up;

    public float Impact => 1f;

    public float ZImpact => 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }



    // Update is called once per frame
    void Update()
    {
        if (!isActive || outOfWater)
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
