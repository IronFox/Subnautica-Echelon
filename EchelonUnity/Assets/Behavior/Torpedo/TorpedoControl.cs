using UnityEngine;

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

    public CameraShake CameraShake { get; set; }

    public Collider normalCollider;

    public static TorpedoTerrainCollisions terrainCollisions = TorpedoTerrainCollisions.IgnoreWhenTargeted;

    //public float detonationProximity = 1;



    public Rigidbody origin;
    public float safetyOriginDistance = 5;

    public int techLevel;

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

            enabled = value;
        }

    }



    // Start is called before the first frame update
    void Start()
    {
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
        Detonator.origin = origin;
        Detonator.techLevel = techLevel;
        Detonator.cameraShake = CameraShake;
        TorpedoDirectAt = GetComponent<TorpedoDirectAt>();
        ParticleSystem = GetComponentInChildren<ParticleSystem>();
        SoundAdapter = GetComponent<SoundAdapter>();
        Drive = GetComponent<TorpedoDrive>();
        Drive.origin = origin;
        CollisionTrigger = GetComponentInChildren<CollisionTrigger>();
        CollisionTrigger.doNotCollideWith = origin;
        CollisionTrigger.target = TargetPredictor.target;

        Lights = GetComponentsInChildren<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        Detonator.cameraShake = CameraShake;
        ProximityDetector.doNotCollideWith = origin;
        CollisionTrigger.doNotCollideWith = origin;
        Detonator.origin = origin;
        Detonator.techLevel = techLevel;
        Drive.origin = origin;
        CollisionTrigger.target = TargetPredictor.target;

        if (normalCollider.isTrigger && !ProximityDetector.IsIntersectingWithExclusion
            && (origin != null && Vector3.Distance(transform.position, origin.position) > safetyOriginDistance)
            )
        {
            normalCollider.isTrigger = false;
            Drive.triggerActive = false;
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

public enum TorpedoTerrainCollisions
{
    NeverIgnore,
    IgnoreWhenTargeted,
    AlwaysIgnore
}
