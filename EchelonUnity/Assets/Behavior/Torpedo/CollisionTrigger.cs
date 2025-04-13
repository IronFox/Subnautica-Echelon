using UnityEngine;

public class CollisionTrigger : MonoBehaviour
{
    public Collider regularCollider;
    public Detonator detonator;
    public Rigidbody doNotCollideWith;
    public ITargetable target;

    public static string[] SmallFishNames = new string[]{
        "Eyeye",
        "Boomerang",
        "JellyRay",
        "GarryFish",
        "Peeper",
        "RabbitRay",
        "Bubble",
        "Floater",
        "Crash",
        "BladderFish",
        "Reginald",
        "SpadeFish",
        "HoopFish",
        "HoleFish",
        "Biter",
        "Reginald",
        "HoverFish",
    };

    public static string[] PassiveFishNames = new string[] {
        "GhostRayRed",
        "LavaLarva",
        "Reefback",

    };

    // Start is called before the first frame update
    void Start()
    {
        //regularCollider = GetComponent<Collider>();
    }
    void OnCollisionEnter(Collision collision)
    {



        //if (collision.gameObject.name.StartsWith("Cube")
        //    || collision.gameObject.name.StartsWith("Sphere")
        //    )
        //    return; //no idea what these are, but let's ignore them
        //if (SmallFishNames.Any(x => collision.gameObject.name.StartsWith(x)))
        //    return; //let's not detonate with these

        if (PhysicsHelper.CanCollide(collision.collider, regularCollider, doNotCollideWith) && collision.rigidbody)
        {
            var t = TargetAdapter.ResolveTarget(collision.rigidbody.gameObject, collision.rigidbody);

            bool ignoreNonTargets = false;
            switch (TorpedoControl.terrainCollisions)
            {
                case TorpedoTerrainCollisions.IgnoreWhenTargeted:
                    ignoreNonTargets = (target is AdapterTargetable);
                    break;
                case TorpedoTerrainCollisions.AlwaysIgnore:
                    ignoreNonTargets = true;
                    break;
                case TorpedoTerrainCollisions.NeverIgnore:
                    break;
            }
            if (t == null)
            {
                if (ignoreNonTargets || collision.rigidbody.gameObject.GetComponent<TorpedoControl>() != null)
                {
                    Physics.IgnoreCollision(collision.collider, regularCollider);
                    return;
                }
            }

            if (t != null && t.MaxHealth < 200 && !IsTarget(collision.collider.attachedRigidbody.gameObject))
            {
                ULog.Write($"Colliding instance {t} is too fragile: Ramming & ignoring");
                t.DealDamage(transform.position, 100, gameObject);
                return;
            }

            ULog.Write($"Reacting to collision with {collision.collider} (health={t?.MaxHealth}): Detonating");
            detonator.Detonate();
        }
        else
            ULog.Write($"Ignoring collision with {collision.collider} (cannot collide)");
    }

    private bool IsTarget(GameObject gameObject)
    {
        if (target == null)
            return false;
        return target.Is(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
