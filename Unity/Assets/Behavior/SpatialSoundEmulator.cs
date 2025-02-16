using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialSoundEmulator : MonoBehaviour
{
    private AudioSource source;
    private float minDistance=1;
    private float maxDistance=500;
    public float halfDistance = 10f;
    private Vector3 lastCameraPosition, cameraMotionPerSecond;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.spatialBlend = 0;
        minDistance = source.minDistance;
        maxDistance = source.maxDistance;
        lastCameraPosition = Camera.main.transform.position;
    }

    void FixedUpdate()
    {
        cameraMotionPerSecond = (Camera.main.transform.position - lastCameraPosition) / Time.fixedDeltaTime;
        lastCameraPosition = Camera.main.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        var delta = transform.position - Camera.main.transform.position;
        var distance = delta.magnitude;
        var soundDirection = delta / distance;

        var cameraSpeedDirectional = M.Dot(soundDirection, cameraMotionPerSecond);
        var doppler = cameraSpeedDirectional / 300f;
        source.pitch = Mathf.Pow(2f, doppler) ;

        float right = M.Dot(Camera.main.transform.right, soundDirection);

        source.panStereo = right;
        if (distance > maxDistance)
        {
            source.volume = 0;
        }
        else if (distance < minDistance)
        {
            source.volume = 1;
        }
        else
        {
            distance -= minDistance;
            distance /= halfDistance / M.Sqrt2;
            source.dopplerLevel = 2;
            source.volume = 1f / (distance*distance) - (1f / (maxDistance*maxDistance));
            //Debug.Log($"Volume @{distance} now {source.volume}");
        }

    }
}
