﻿using UnityEngine;

public class SpatialSoundEmulator : MonoBehaviour
{
    private AudioSource source;
    private float minDistance = 1;
    private float maxDistance = 500;
    public float halfDistance = 10f;
    public float pitch = 1f;
    public float volume = 1f;
    private Transform lastCamera;
    private Vector3 lastCameraPosition, cameraMotionPerSecond;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.spatialBlend = 0;
        minDistance = source.minDistance;
        maxDistance = source.maxDistance;
        lastCamera = CameraUtil.GetTransform(nameof(SpatialSoundEmulator));
        if (lastCamera != null)
            lastCameraPosition = lastCamera.position;
    }



    // Update is called once per frame
    void Update()
    {
        lastCamera = CameraUtil.GetTransform(nameof(SpatialSoundEmulator));
        if (lastCamera != null)
        {
            if (lastCameraPosition != null && Time.deltaTime > 0)
            {
                cameraMotionPerSecond = (lastCamera.position - lastCameraPosition) / Time.deltaTime;
                lastCameraPosition = lastCamera.position;
            }

            var delta = transform.position - lastCamera.position;
            var distance = delta.magnitude;
            var soundDirection = delta / distance;

            var cameraSpeedDirectional = M.Dot(soundDirection, cameraMotionPerSecond);
            var doppler = cameraSpeedDirectional / 300f;
            source.pitch = Mathf.Pow(2f, doppler) * pitch;

            float right = M.Dot(lastCamera.right, soundDirection);

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
                source.volume = volume * (1f / (distance * distance) - (1f / (maxDistance * maxDistance)));
            }
        }
    }
}
