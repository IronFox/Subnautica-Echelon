using System;
using UnityEngine;

public class TorpedoLaunchControl : MonoBehaviour
{
    public GameObject torpedoPrefab;
    public float relativeExitVelocity = 200;
    public Transform cover;
    public float secondsToOpenCover => (60 / TorpedoesPerMinute) * 0.25f;
    public float secondsToFire => (60 / TorpedoesPerMinute) * 0.5f;
    public Transform coverOpenPosition;
    public bool noExplosions;
    public float overrideMaxLifetimeSeconds;
    public SoundAdapter fireSound;

    /// <summary>
    /// Zero-based torpedo tech level
    /// </summary>
    public int torpedoTechLevel;
    public float TorpedoesPerMinute => 20 + torpedoTechLevel * 12;

#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable CS0414
    private bool everOpened;
#pragma warning restore CS0414
#pragma warning restore IDE0052 // Remove unread private members

    private Torpedo torpedoInTube;

    public float CycleTime => secondsToOpenCover * 2 + secondsToFire;
    public float CycleProgress => coverRedactionProgress + fireRecoverProgress + coverRecoveryProgress;

    public ITargetable FireWithTarget { get; set; }

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
        try
        {

            if (fired)
            {
                fireRecoverProgress += Time.deltaTime;
                fireSound.play = true;
                fireSound.volume = M.Saturate(1f - fireRecoverProgress / Mathf.Min(1, secondsToFire));
                if (fireRecoverProgress > secondsToFire)
                {
                    fireRecoverProgress = secondsToFire;
                    fired = false;
                    coverRecoveryProgress = 0;
                    SetCover(1);
                    closing = true;
                }

            }
            else if (closing)
            {
                fireSound.play = false;
                coverRecoveryProgress += Time.deltaTime;
                if (coverRecoveryProgress > secondsToOpenCover)
                {
                    closing = false;

                    SetCover(0);
                    fireRecoverProgress = 0;
                    coverRecoveryProgress = 0;
                    coverRedactionProgress = 0;
                }
                else
                    SetCover(1f - coverRecoveryProgress / secondsToOpenCover);
            }
            else if (FireWithTarget != null)
            {
                //openSound.play = true;
                fireSound.play = false;
                if (torpedoInTube == null)
                    torpedoInTube = InstantiateTorpedo();

                coverRedactionProgress += Time.deltaTime;
                everOpened = true;
                if (coverRedactionProgress > secondsToOpenCover)
                {
                    //openSound.play = false;
                    fireSound.play = true;
                    coverRedactionProgress = secondsToOpenCover;
                    SetCover(1);
                    fired = true;

                    torpedoInTube.Launch(
                        myBody.GetPointVelocity(transform.position) + transform.forward * relativeExitVelocity,
                        FireWithTarget,
                        noExplosions,
                        overrideMaxLifetimeSeconds);
                    lastTorpedo = torpedoInTube;
                    torpedoInTube = null;

                }
                else
                    SetCover(coverRedactionProgress / secondsToOpenCover);
            }
            else
            {
                everOpened = false;
                fireSound.play = false;
                if (coverRedactionProgress > 0)
                {
                    coverRedactionProgress -= Time.deltaTime;
                    if (coverRedactionProgress < 0)
                    {
                        coverRedactionProgress = 0;
                        SetCover(0);
                        if (torpedoInTube != null)
                        {
                            ULog.Write("Releasing unneeded torpedo in tube");

                            torpedoInTube.Destroy();
                            torpedoInTube = null;
                        }
                    }
                    else
                    {
                        coverRedactionProgress = M.Max(coverRedactionProgress, 0);
                        SetCover(coverRedactionProgress / secondsToOpenCover);
                    }
                }

            }
        }
        catch (Exception e)
        {
            ULog.Exception($"TorpedoLaunchControl.Update()", e, gameObject);
        }

    }

    private Torpedo InstantiateTorpedo()
    {
        var torpedo = Instantiate(torpedoPrefab, transform);
        return new Torpedo(myBody, transform, torpedo, torpedoTechLevel);

    }


}


public class Torpedo
{

    public bool IsAlive => GameObject != null;
    public void Launch(Vector3 velocity, ITargetable target, bool noExplosions, float overrideMaxFlightTime)
    {
        ULog.Write($"Launching torpedo at {target}");
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

    public void Destroy()
    {
        GameObject.Destroy(GameObject);
    }

    public Torpedo(Rigidbody origin, Transform owner, GameObject torpedo, int techLevel)
    {
        GameObject = torpedo;

        Control = torpedo.GetComponent<TorpedoControl>();
        Control.origin = origin;
        Control.techLevel = techLevel;
        Control.IsLive = false;
        torpedo.transform.localPosition = Vector3.zero;
        //this compensates a bit that torpedoes are crossly misplaced at high velocities:
        torpedo.transform.position += origin.GetPointVelocity(owner.position) * 0.025f;

        torpedo.transform.localEulerAngles = Vector3.zero;
    }

    public GameObject GameObject { get; }
    public TorpedoControl Control { get; }

}