using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetScanner : MonoBehaviour
{
    public float minDistance = 1;
    public float maxDistance = 500;
    public float minWidth = 0.6f;
    public float minHeight = 0.4f;

    public float lastScanTime = 0;

    private float lastMinDistance;
    private float lastMaxDistance;
    private float lastWidth;
    private float lastHeight;


    private readonly CountedSet<RigidbodyReference> viableTargets 
        = new CountedSet<RigidbodyReference>();



    private MeshCollider targetCollider;

    private Mesh mesh;

    //private void Rebuild()
    //{
    //    if (mesh == null)
    //        mesh = new Mesh();



    //    float w0 = minWidth / 2;
    //    float h0 = minHeight / 2;
    //    float w1 = w0 / minDistance * maxDistance;
    //    float h1 = h0 / minDistance * maxDistance;

    //    mesh.vertices = new Vector3[]
    //        {
    //             M.V3(-w0,-h0, minDistance),    //0
    //             M.V3(w0,-h0, minDistance),     //1
    //             M.V3(w0,h0, minDistance),      //2
    //             M.V3(-w0,h0, minDistance),     //3
    //             M.V3(-w1,-h1, maxDistance),    //4
    //             M.V3(w1,-h1, maxDistance),     //5
    //             M.V3(w1,h1, maxDistance),      //6
    //             M.V3(-w1,h1, maxDistance),     //7
    //        };

    //    mesh.subMeshCount = 1;
    //    mesh.SetTriangles(new int[]{
    //        0,1,2, 0,2,3,
    //        3,2,6, 3,6,7,
    //        2,1,5, 2,5,6,
    //        1,0,4, 1,4,5,
    //        0,3,7, 0,7,4,
    //        4,5,7, 5,7,6,
    //    }, 0);

    //    targetCollider.sharedMesh = mesh;
    //}

    // Start is called before the first frame update
    void Start()
    {
        targetCollider = GetComponent<MeshCollider>();
    }

    private string[] excludePrefixes = new string[]{
        "Coral_reef_",
        "Reefback",
        "BrainCoral",
        "ReefbackBaby",
        "EscapePod",
        "ThermalPlant",
        "CuteFish",
    };

    private string[] alwaysTarget = new string[] {
        "Warper"
    };

    private bool IsExcludedByName(string objectName)
    {
        return excludePrefixes.Any(x => objectName.StartsWith(x))
            || objectName.Contains("School")
            
            ;
    }

    private bool AlwaysIncludeByName(string objectName)
    {
        return alwaysTarget.Any(x => objectName.StartsWith(x));
    }

    public TargetAdapter GetBestTarget(Transform exclude)
    {
        var started = DateTime.Now;

        Ray ray = new Ray(transform.position, transform.forward);

        float closestDist = float.MaxValue;
        TargetAdapter closest = null;

        float at = minDistance;
        while (at < maxDistance)
        {
            float w = minWidth / minDistance * at;
            float h = minDistance / minDistance * at;
            float d = Mathf.Min(w, h);
            
            var candidates = Physics.OverlapSphere(ray.GetPoint(at), d / 2);
            foreach (var item in candidates)
            {
                if (item.attachedRigidbody == null || item.isTrigger || !item.enabled)
                    continue;
                if (exclude != null && item.transform.IsChildOf(exclude))
                    continue;
                if (item.attachedRigidbody.transform.GetComponent<TorpedoControl>() != null)
                    continue;
                var t = TargetAdapter.ResolveTarget(item.attachedRigidbody.gameObject, item.attachedRigidbody);
                if (t is null || t.IsInvincible || !t.IsAlive)
                    continue;
                if (t.MaxHealth < 200 && !AlwaysIncludeByName(item.attachedRigidbody.gameObject.name))
                    continue;
                if (IsExcludedByName(item.attachedRigidbody.gameObject.name))
                    continue;

                //var distance = M.SqrDistance(transform.position, item.Rigidbody.transform.position);
                var rayDistance = M.Distance(ray, item.transform.position);
                var distance = rayDistance.DistanceAlongRay + rayDistance.DistanceToClosesPointOnRay * 10;

                if (distance < minDistance)
                    continue;
                if (closest == null || distance < closestDist)
                {
                    closest = t;
                    closestDist = distance;
                }
            }
            at += d;
        }

        var elapsed = DateTime.Now - started;
        lastScanTime = (float)elapsed.TotalSeconds;

        return closest;
    }

    // Update is called once per frame
    void Update()
    {
        minHeight = Mathf.Tan(M.DegToRad(Camera.main.fieldOfView)) 
            * minDistance
            * 0.1f;
        minWidth = minHeight * Camera.main.aspect;


        if (lastWidth != minWidth
            || lastHeight != minHeight
            || lastMaxDistance != maxDistance
            || lastMinDistance != minDistance
            )
        {
            lastWidth = minWidth;
            lastHeight = minHeight;
            lastMaxDistance = maxDistance;
            lastMinDistance = minDistance;
            //Rebuild();
        }
    }
    

}


internal readonly struct RigidbodyReference
{
    public Rigidbody Rigidbody { get; }
    public GameObject GameObject { get; }
    public int InstanceId { get; }
    public bool IsAlive => Rigidbody != null;

    public RigidbodyReference(Rigidbody source)
    {
        Rigidbody = source;
        GameObject = Rigidbody.gameObject;
        InstanceId = GameObject.GetInstanceID();
    }


    public override bool Equals(object obj)
    {
        return obj is RigidbodyReference that
            && that.InstanceId == this.InstanceId;
    }

    public override int GetHashCode()
    {
        return -676353417 + InstanceId.GetHashCode();
    }
}
