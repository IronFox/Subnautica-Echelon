using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollisionTrigger : MonoBehaviour
{
    public Collider regularCollider;
    public Detonator detonator;
    public Rigidbody doNotCollideWith;

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


        if (PhysicsHelper.CanCollide(collision.collider, regularCollider, doNotCollideWith))
        {
            var t = TargetAdapter.ResolveTarget(collision.gameObject, collision.rigidbody);
            if (t != null && t.MaxHealth < 100)
            {
                ConsoleControl.Write($"Colliding instance {t} is too fragile: Killing & ignoring");
                t.DealDamage(transform.position, 100, gameObject);
                return;
            }

            ConsoleControl.Write($"Reacting to collision with {collision.collider}: Detonating");
            detonator.Detonate();
        }
        else
            ConsoleControl.Write($"Ignoring collision with {collision.collider}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
