using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TorpedoControl : MonoBehaviour
{
    public TargetPredictor TargetPredictor { get; private set; }
    public MaxFlightTime MaxFlightTime { get; private set; }
    public Detonator Detonator { get; private set; }
    public TorpedoDirectAt TorpedoDirectAt { get; private set; }
    public ParticleSystem ParticleSystem { get; private set; }
    public SoundAdapter SoundAdapter { get; private set; }

    public TurnPropeller TurnPropeller { get; private set; }
    public ProximityDetector ProximityDetector { get; private set; }
    public TorpedoTargeting Targeting { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public TorpedoDrive Drive { get; private set; }
    public CollisionTrigger CollisionTrigger { get; private set; }
    public Light[] Lights { get; private set; }

    public Collider normalCollider;

    public static bool ignoreNonTargetCollisions = false;

    //public float detonationProximity = 1;



    public Rigidbody origin;
    public float safetyOriginDistance = 10;


    public bool IsLive
    {
        get
        {
            return enabled;
        }
        set
        {
            if (Rigidbody == null)
                LoadComponents(); //this has just been instantiated

            Rigidbody.isKinematic = !value;
            TargetPredictor.enabled = value;
            MaxFlightTime.enabled = value;
            ProximityDetector.enabled = value;
            CollisionTrigger.enabled = value;
            Detonator.enabled = value;
            Targeting.enabled = value;
            TorpedoDirectAt.enabled = value;
            var em = ParticleSystem.emission;
            em.enabled = value;
            SoundAdapter.play = value;
            TurnPropeller.enabled = value;
            Drive.enabled = value;

            foreach (var light in Lights)
                light.enabled = value;

            Debug.Log("Targeting.enabled := " + Targeting.enabled);

            enabled = value;
        }

    }



    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Torpedo: start()");
        LoadComponents();
    }

    private void LoadComponents()
    {
        TargetPredictor = GetComponent<TargetPredictor>();
        TurnPropeller = GetComponent<TurnPropeller>();
        ProximityDetector = GetComponentInChildren<ProximityDetector>();
        Targeting = GetComponent<TorpedoTargeting>();
        ProximityDetector.doNotCollideWith = origin;
        Rigidbody = GetComponent<Rigidbody>();
        MaxFlightTime = GetComponent<MaxFlightTime>();
        Detonator = GetComponent<Detonator>();
        TorpedoDirectAt = GetComponent<TorpedoDirectAt>();
        ParticleSystem = GetComponentInChildren<ParticleSystem>();
        SoundAdapter = GetComponent<SoundAdapter>();
        Drive = GetComponent<TorpedoDrive>();
        Drive.origin = origin;
        CollisionTrigger = GetComponentInChildren<CollisionTrigger>();
        CollisionTrigger.doNotCollideWith = origin;
        CollisionTrigger.target = TargetPredictor.target;
        Detonator.origin = origin;
        Lights = GetComponentsInChildren<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        ProximityDetector.doNotCollideWith = origin;
        CollisionTrigger.doNotCollideWith = origin;
        Detonator.origin = origin;
        Drive.origin = origin;
        CollisionTrigger.target = TargetPredictor.target;

        if (normalCollider.isTrigger && !ProximityDetector.IsIntersectingWithExclusion
            && (origin != null && Vector3.Distance(transform.position, origin.position) > safetyOriginDistance)
            )
        {
            ConsoleControl.Write($"Exited exlusion intersection. Restoring collider");
            normalCollider.isTrigger = false;
        }
    }

    void FixedUpdate()
    {
        if (ActorAdapter.IsOutOfWater(gameObject, Rigidbody.position))
        {
            Detonator.Detonate();
        }
    }



}
