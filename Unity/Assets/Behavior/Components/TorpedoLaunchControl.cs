using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoLaunchControl : MonoBehaviour
{
    public GameObject torpedoPrefab;
    public float relativeExitVelocity=200;
    public Transform cover;
    public float secondsToOpenCover = 0.5f;
    public float secondsToFire = 1f;
    public Transform coverOpenPosition;
    public bool noExplosions;
    public float overrideMaxLifetimeSeconds;
    public SoundAdapter openSound;
    public SoundAdapter fireSound;

    private Torpedo torpedoInTube;

    public float CycleTime => secondsToOpenCover * 2 + secondsToFire;
    public float CycleProgress => coverRedactionProgress + fireRecoverProgress + coverRecoveryProgress;

    public ITargetable fireWithTarget;

    private TransformDescriptor originalCoverPosition;
    private TransformDescriptor openCoverPosition;

    private float coverRedactionProgress;
    private float coverRecoveryProgress;
    private float fireRecoverProgress;
    private bool fired;
    private bool closing;
    public Rigidbody myBody;
    private Torpedo lastTorpedo;

    // Start is called before the first frame update
    void Start()
    {
        originalCoverPosition = TransformDescriptor.FromLocal(cover);
        openCoverPosition = TransformDescriptor.FromLocal(coverOpenPosition);
    }

    private void SetCover(float at)
    {
        var position = TransformDescriptor.Lerp(originalCoverPosition, openCoverPosition, at);
        position.ApplyTo(cover);
    }
    // Update is called once per frame
    void Update()
    {
        if (fired)
        {
            openSound.play = false;
            fireRecoverProgress += Time.deltaTime;
            fireSound.play = true;
            fireSound.volume = 1f - fireRecoverProgress / secondsToFire;
            //Debug.Log("Waiting for fire recovery @" + fireRecoverProgress);
            if (fireRecoverProgress > secondsToFire)
            {
                fireRecoverProgress = secondsToFire;
                Debug.Log("Recovered from firing. Closing again");
                fired = false;
                coverRecoveryProgress = 0;
                SetCover(1);
                closing = true;
            }

        }
        else if (closing)
        {
            openSound.play = true;
            fireSound.play = false;
            coverRecoveryProgress += Time.deltaTime;
            //Debug.Log("Closing again @" + coverRecoveryProgress);
            if (coverRecoveryProgress > secondsToOpenCover)
            {
                Debug.Log("Closed");
                closing = false;

                SetCover(0);
                fireRecoverProgress = 0;
                coverRecoveryProgress = 0;
                coverRedactionProgress = 0;
            }
            else
                SetCover(1f - coverRecoveryProgress / secondsToOpenCover);
        }
        else if (fireWithTarget != null)
        {
            openSound.play = true;
            fireSound.play = false;
            if (torpedoInTube == null)
                torpedoInTube = InstantiateTorpedo();

            coverRedactionProgress += Time.deltaTime;
            //Debug.Log("Opening @"+ coverRedactionProgress);
            if (coverRedactionProgress > secondsToOpenCover)
            {
                openSound.play = false;
                fireSound.play = true;
                coverRedactionProgress = secondsToOpenCover;
                SetCover(1);
                Debug.Log("Firing");
                fired = true;

                torpedoInTube.Launch(
                    myBody.GetPointVelocity(transform.position) + transform.forward * relativeExitVelocity,
                    fireWithTarget,
                    noExplosions,
                    overrideMaxLifetimeSeconds);
                lastTorpedo = torpedoInTube;
                Debug.Log("Releasing old torpedo");
                torpedoInTube = null;

            }
            else
                SetCover(coverRedactionProgress / secondsToOpenCover);
        }
        else
        {
            openSound.play = false;
            fireSound.play = false;
            if (coverRedactionProgress > 0)
            {
                openSound.play = true;

                coverRedactionProgress -= Time.deltaTime;
                coverRedactionProgress = M.Max(coverRedactionProgress, 0);
                //Debug.Log("Closing @" + coverRedactionProgress);
                SetCover(coverRedactionProgress / secondsToOpenCover);
            }

        }

   }

    private Torpedo InstantiateTorpedo()
    {
        Debug.Log($"Creating torpedo");
        var torpedo = Instantiate(torpedoPrefab, transform);
        return new Torpedo(myBody, torpedo);

    }
}


public class Torpedo
{

    public bool IsAlive => GameObject != null;
    public void Launch(Vector3 velocity, ITargetable target, bool noExplosions, float overrideMaxFlightTime)
    {
        Control.Rigidbody.velocity = velocity;
        GameObject.transform.parent = null;
        Control.Detonator.noExplosion = noExplosions;
        Control.TargetPredictor.target = target;


        if (overrideMaxFlightTime > 0)
            Control.MaxFlightTime.maxLifetimeSeconds = overrideMaxFlightTime;
        Control.IsLive = true;


        //debug keep stationary
        //Control.MaxFlightTime.enabled = false;
        //Control.acceleration = 0.01f;
        //Control.minAcceleration = 0.001f;
    }

    public Torpedo(Rigidbody origin, GameObject torpedo)
    {
        GameObject = torpedo;

        Control = torpedo.GetComponent<TorpedoControl>();
        Control.origin = origin;
        Control.IsLive = false;
        torpedo.transform.localPosition = Vector3.zero;
        torpedo.transform.localEulerAngles = Vector3.zero;

        Debug.Log($"Torpedo created");

    }

    public GameObject GameObject { get; }
    public TorpedoControl Control { get; }

}