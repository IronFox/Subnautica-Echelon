using System;
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

    void Start()
    {
        targetCollider = GetComponent<MeshCollider>();
    }

    private static string[] excludePrefixes = new string[]{
        "Coral_reef_",
        "Reefback",
        "BrainCoral",
        "ReefbackBaby",
        "EscapePod",
        "ThermalPlant",
        "CuteFish",
    };

    private static string[] alwaysTarget = new string[] {
        "Warper"
    };

    public static bool IsExcludedByName(string objectName)
    {
        return excludePrefixes.Any(x => objectName.StartsWith(x))
            || objectName.Contains("School")
            
            ;
    }

    public static bool AlwaysIncludeByName(string objectName)
    {
        return alwaysTarget.Any(x => objectName.StartsWith(x));
    }




    public TargetAdapter GetBestTarget(ReadOnlyTargetEnvironment env)
    {
        var started = DateTime.Now;

        Ray ray = new Ray(transform.position, transform.forward);

        float closestDist = float.MaxValue;
        TargetAdapter closest = null;
        var reference = M.Distance(ray, env.SensorCenter);

        foreach (var t in env.Targets)
        {
            //var distance = M.SqrDistance(transform.position, item.Rigidbody.transform.position);
            var rayDistance = M.Distance(ray, t.TargetAdapter.GameObject.transform.position);
            if (rayDistance.DistanceAlongRay < reference.DistanceAlongRay)
                continue;
            var distance = rayDistance.DistanceAlongRay + rayDistance.DistanceToClosesPointOnRay * 10;

            if (distance < minDistance)
                continue;
            if (closest == null || distance < closestDist)
            {
                closest = t.TargetAdapter;
                closestDist = distance;
            }
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


public class ReadOnlyTargetEnvironment
{
    protected Collider[] buffer = new Collider[1024];
    protected int numTargets = 0;
    protected readonly List<AdapterTargetable> targets = new List<AdapterTargetable>();
    protected readonly HashSet<int> objectInstanceIds = new HashSet<int>();
    public Vector3 SensorCenter {get; protected set; }
    public IReadOnlyList<AdapterTargetable> Targets => targets;

    public bool IsTarget(int objectInstanceId)
        => objectInstanceIds.Contains(objectInstanceId);
    protected ReadOnlyTargetEnvironment()
    { }
}

public class TargetEnvironment : ReadOnlyTargetEnvironment
{
    public void Update(Vector3 position, float radius, params Transform[] exclude)
    {
        SensorCenter = position;
        numTargets = Physics.OverlapSphereNonAlloc(position, radius, buffer);
        while (numTargets >= buffer.Length)
        {
            buffer = new Collider[buffer.Length * 2];
            ConsoleControl.Write($"Increased target environment buffer size to {buffer.Length}");
            numTargets = Physics.OverlapSphereNonAlloc(position, radius, buffer);
        }

        objectInstanceIds.Clear();
        targets.Clear();
        for (int i = 0; i < numTargets; i++)
        {
            var item = buffer[i];

            if (item.attachedRigidbody == null || item.isTrigger || !item.enabled)
                continue;
            if (exclude.Length > 0 && exclude.Any(ex => item.transform.IsChildOf(ex)))
                continue;
            if (item.attachedRigidbody.transform.GetComponent<TorpedoControl>() != null)
                continue;
            var t = TargetAdapter.ResolveTarget(item.attachedRigidbody.gameObject, item.attachedRigidbody);
            if (t is null || t.IsInvincible || !t.IsAlive)
                continue;
            if (t.MaxHealth < 200 && !TargetScanner.AlwaysIncludeByName(item.attachedRigidbody.gameObject.name))
                continue;
            if (TargetScanner.IsExcludedByName(item.attachedRigidbody.gameObject.name))
                continue;
            targets.Add(new AdapterTargetable(t));
            objectInstanceIds.Add(t.GameObjectInstanceId);
        }
    }


}

/// <summary>
/// Preserve instances mapped to targets, preferrably from a ReadOnlyTargetEnvironment
/// </summary>
/// <typeparam name="T">Type that is to be associated with each one living target</typeparam>
public class TargetPool<T>
{
    private readonly Dictionary<int,T> map = new Dictionary<int,T>();

    /// <summary>
    /// Constructs a new pool
    /// </summary>
    /// <param name="getGameObject">Function to resolve the game object of an instance (for destruction)</param>
    /// <param name="instantiate">Function to create a new instance for a target not previously mapped</param>
    public TargetPool(
        Func<T, GameObject> getGameObject,
        Func<AdapterTargetable, T> instantiate
        )
    {
        GetGameObject = getGameObject;
        Instantiate = instantiate;
    }

    public Func<T, GameObject> GetGameObject { get; }
    public Func<AdapterTargetable, T> Instantiate { get; }

    /// <summary>
    /// Removes a single instance from the local map
    /// </summary>
    /// <param name="gameObjectInstanceId"></param>
    private void Flush(int gameObjectInstanceId)
    {
        if (map.TryGetValue(gameObjectInstanceId, out var instance))
        {
            try
            {
                GameObject.Destroy(GetGameObject(instance));
            }
            catch (Exception ex)
            {
                ConsoleControl.WriteException( nameof(TargetPool<T>)+ ".Flush("+gameObjectInstanceId+")", ex);
            }
            finally
            {
                map.Remove(gameObjectInstanceId);
            }
        }
    }

    /// <summary>
    /// Completely flushes the local pool.
    /// All instances associated with targets are destroyed.
    /// </summary>
    public void Clear()
    {
        foreach (var p in map)
        {
            var instance = p.Value;
            ConsoleControl.Write($"Destroying {typeof(T).Name} {p.Key}");
            try
            {
                GameObject.Destroy(GetGameObject(instance));
            }
            catch (Exception ex)
            {
                ConsoleControl.WriteException(nameof(TargetPool<T>) + ".Flush(" + p.Key + ")", ex);
            }
        }
        map.Clear();
    }

    private readonly HashSet<int> touched = new HashSet<int>();
    private readonly List<KeyValuePair<int, T>> remove = new List<KeyValuePair<int, T>>();

    /// <summary>
    /// Maps a set of targets to instances
    /// </summary>
    /// <typeparam name="Status">Transfer type between <paramref name="filter"/> and <paramref name="apply"/></typeparam>
    /// <param name="env">Target source</param>
    /// <param name="filter">
    /// Filter function that produces a non-null value
    /// for consumption by <paramref name="apply"/>
    /// iff an instance should exist for this target,
    /// null if not. If this function returns null,
    /// <paramref name="apply"/> is not called for that target</param>
    /// <param name="apply">
    /// Function to call for existing or newly created instances.
    /// Receives the instance, target, and transfer data from <paramref name="filter"/>
    /// </param>
    public void Map<Status>(
        ReadOnlyTargetEnvironment env,
        Func<AdapterTargetable, Status?> filter,
        Action<T, Status, AdapterTargetable> apply)
        where Status : struct
        => Map(env.Targets, filter, apply);

    /// <summary>
    /// Maps a set of targets to instances
    /// </summary>
    /// <typeparam name="Status">Transfer type between <paramref name="filter"/> and <paramref name="apply"/></typeparam>
    /// <param name="targets">Target source</param>
    /// <param name="filter">
    /// Filter function that produces a non-null value
    /// for consumption by <paramref name="apply"/>
    /// iff an instance should exist for this target,
    /// null if not. If this function returns null,
    /// <paramref name="apply"/> is not called for that target</param>
    /// <param name="apply">
    /// Function to call for existing or newly created instances.
    /// Receives the instance, target, and transfer data from <paramref name="filter"/>
    /// </param>
    public void Map<Status>(
        IEnumerable<AdapterTargetable> targets,
        Func<AdapterTargetable, Status?> filter,
        Action<T, Status, AdapterTargetable> apply)
        where Status : struct
    {
        try
        {
            remove.Clear();
            touched.Clear();
            foreach (var t in targets)
            {
                var goid = t.TargetAdapter.GameObjectInstanceId;
                var status = filter(t);
                if (status is null)
                {
                    Flush(goid);
                    continue;
                }
                if (!map.TryGetValue(goid, out var instance))
                {
                    try
                    {
                        ConsoleControl.Write($"Creating {typeof(T).Name} for target {t}");
                        instance = Instantiate(t);
                        map.Add(goid, instance);
                    }
                    catch (Exception ex)
                    {
                        ConsoleControl.WriteException(nameof(TargetPool<T>) + $".Instantiate({t})", ex);
                        continue;
                    }
                }
                apply(instance, status.Value, t);
                touched.Add(goid);
            }

            remove.AddRange(map.Where(x => !touched.Contains(x.Key)));

            foreach (var p in remove)
            {
                var instance = p.Value;
                ConsoleControl.Write($"Destroying {typeof(T).Name} {p.Key}");
                try
                {
                    GameObject.Destroy(GetGameObject(instance));
                }
                catch (Exception ex)
                {
                    ConsoleControl.WriteException(nameof(TargetPool<T>) + ".Flush(" + p.Key + ")", ex);
                }
                finally
                {
                    map.Remove(p.Key);
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleControl.WriteException(nameof(TargetPool<T>) + ".Map()", ex);

        }
    }
}