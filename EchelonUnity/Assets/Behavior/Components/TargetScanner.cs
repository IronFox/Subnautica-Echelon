﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetScanner : MonoBehaviour
{
    public float minDistance = 1;
    public float maxDistance = 500;

    public float lastScanTime = 0;


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
        "SeaTreader",
        "MapRoomCamera",
    };

    private static string[] alwaysTarget = new string[] {
        "Warper",
    };

    public static bool IsExcludedByName(string objectName)
    {
        return excludePrefixes.Any(x => objectName.StartsWith(x))
            || objectName.Contains("School")

            ;
    }
    public static bool IsCriticalTarget(GameObject gameObject)
        => AlwaysIncludeByName(gameObject.name);
    public static bool AlwaysIncludeByName(string objectName)
    {
        return alwaysTarget.Any(x => objectName.StartsWith(x));
    }




    public TargetAdapter GetBestTarget(ReadOnlyTargetEnvironment env)
    {
        try
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
                if (rayDistance.DistanceAlongRay * 0.3f < rayDistance.DistanceToClosesPointOnRay)
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
        catch (Exception ex)
        {
            ULog.Exception(nameof(GetBestTarget), ex, gameObject);
            return null;
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



public class UpdateProcess
{
    public UpdateProcess(
        TargetEnvironment targetEnvironment,
        Collider[] buffer,
        int numTargets,
        Transform[] exclude)
    {
        TargetEnvironment = targetEnvironment;
        Buffer = buffer;
        NumTargets = numTargets;
        Exclude = exclude;
        TargetEnvironment.Reset();
    }

    public TargetEnvironment TargetEnvironment { get; }
    public Collider[] Buffer { get; }
    public int NumTargets { get; }
    public Transform[] Exclude { get; }

    private int at;


    public bool Next(TimeSpan budget)
    {
        DateTime started = DateTime.Now;
        for (; at < NumTargets && DateTime.Now - started < budget; at++)
        {
            var item = Buffer[at];
            if (item == null)
                continue;
            if (item.attachedRigidbody == null || item.isTrigger || !item.enabled)
                continue;
            if (Exclude.Length > 0 && Exclude.Any(ex => item.transform.IsChildOf(ex)))
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
            TargetEnvironment.Add(new AdapterTargetable(t));
        }
        if (at >= NumTargets)
        {
            TargetEnvironment.Release(this);
            return false;
        }
        return true;
    }



}

public class TargetEnvironment
{
    private UpdateProcess currentProcess;
    protected Collider[] buffer = new Collider[1024];
    protected int numTargets = 0;
    protected readonly List<AdapterTargetable> targets = new List<AdapterTargetable>();
    public Vector3 SensorCenter { get; protected set; }
    public IReadOnlyList<AdapterTargetable> Targets => targets;


    public UpdateProcess Update(Vector3 position, float radius, params Transform[] exclude)
    {
        if (currentProcess != null)
            return currentProcess;
        SensorCenter = position;
        numTargets = Physics.OverlapSphereNonAlloc(position, radius, buffer);
        while (numTargets >= buffer.Length)
        {
            buffer = new Collider[buffer.Length * 2];
            ULog.Write($"Increased target environment buffer size to {buffer.Length}");
            numTargets = Physics.OverlapSphereNonAlloc(position, radius, buffer);
        }
        return currentProcess = new UpdateProcess(this, buffer, numTargets, exclude);

    }

    internal void Add(AdapterTargetable adapterTargetable)
    {
        targets.Add(adapterTargetable);
    }

    internal void Release(UpdateProcess updateProcess)
    {
        currentProcess = null;
    }

    internal void Reset()
    {
        targets.Clear();
    }
}

/// <summary>
/// Preserve instances mapped to targets, preferrably from a ReadOnlyTargetEnvironment
/// </summary>
/// <typeparam name="T">Type that is to be associated with each one living target</typeparam>
public class TargetPool<T>
{
    private readonly Dictionary<int, T> map = new Dictionary<int, T>();

    /// <summary>
    /// Constructs a new pool
    /// </summary>
    /// <param name="destroy">Action to destroy an instance when removed from the local pool</param>
    /// <param name="instantiate">Function to create a new instance for a target not previously mapped</param>
    public TargetPool(
        Action<T, bool> destroy,
        Func<ITargetable, T> instantiate
        )
    {
        Destroy = destroy;
        Instantiate = instantiate;
    }

    public Action<T, bool> Destroy { get; }
    public Func<ITargetable, T> Instantiate { get; }

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
                Destroy(instance, false);
            }
            catch (Exception ex)
            {
                ULog.Exception(nameof(TargetPool<T>) + ".Flush(" + gameObjectInstanceId + ")", ex, null);
            }
            finally
            {
                map.Remove(gameObjectInstanceId);
            }
        }
    }

    /// <summary>
    /// Completely flushes the local pool.
    /// All instances associated with targets are destroyed immediately.
    /// </summary>
    public void Purge()
    {
        foreach (var p in map)
        {
            var instance = p.Value;
            try
            {
                Destroy(instance, true);
            }
            catch (Exception ex)
            {
                ULog.Exception(nameof(TargetPool<T>) + ".Flush(" + p.Key + ")", ex, null);
            }
        }
        map.Clear();
    }

    private readonly HashSet<int> touched = new HashSet<int>();
    private readonly List<KeyValuePair<int, T>> remove = new List<KeyValuePair<int, T>>();

    /// <summary>
    /// Maps a set of targets to instances
    /// </summary>
    /// <typeparam name="Status">Transfer type between <paramref name="filter"/> and <paramref name="update"/></typeparam>
    /// <param name="env">Target source</param>
    /// <param name="filter">
    /// Filter function that produces a non-null value
    /// for consumption by <paramref name="update"/>
    /// iff an instance should exist for this target,
    /// null if not. If this function returns null,
    /// <paramref name="update"/> is not called for that target</param>
    /// <param name="update">
    /// Function to call for existing or newly created instances.
    /// Receives the instance, target, and transfer data from <paramref name="filter"/>
    /// </param>
    public void FilterAndUpdate<Status>(
        ReadOnlyTargetEnvironment env,
        Func<AdapterTargetable, Status?> filter,
        Action<T, Status, AdapterTargetable> update)
        where Status : struct
        => FilterAndUpdate(env.Targets, filter, update);

    /// <summary>
    /// Maps a set of targets to instances
    /// </summary>
    /// <typeparam name="Status">Transfer type between <paramref name="filter"/> and <paramref name="update"/></typeparam>
    /// <param name="targets">Target source</param>
    /// <param name="filter">
    /// Filter function that produces a non-null value
    /// for consumption by <paramref name="update"/>
    /// iff an instance should exist for this target,
    /// null if not. If this function returns null,
    /// <paramref name="update"/> is not called for that target</param>
    /// <param name="update">
    /// Function to call for existing or newly created instances.
    /// Receives the instance, target, and transfer data from <paramref name="filter"/>
    /// </param>
    public void FilterAndUpdate<Status>(
        IEnumerable<AdapterTargetable> targets,
        Func<AdapterTargetable, Status?> filter,
        Action<T, Status, AdapterTargetable> update)
        where Status : struct
    {
        try
        {
            remove.Clear();
            touched.Clear();
            foreach (var t in targets)
            {
                var goid = t.GameObjectInstanceId;
                if (!t.Exists)
                {
                    Flush(goid);
                    continue;
                }
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
                        instance = Instantiate(t);
                        map.Add(goid, instance);
                    }
                    catch (Exception ex)
                    {
                        ULog.Exception(nameof(TargetPool<T>) + $".Instantiate({t})", ex, null);
                        continue;
                    }
                }
                update(instance, status.Value, t);
                touched.Add(goid);
            }

            remove.AddRange(map.Where(x => !touched.Contains(x.Key)));

            foreach (var p in remove)
            {
                var instance = p.Value;
                try
                {
                    Destroy(instance, false);
                }
                catch (Exception ex)
                {
                    ULog.Exception(nameof(TargetPool<T>) + ".Flush(" + p.Key + ")", ex, null);
                }
                finally
                {
                    map.Remove(p.Key);
                }
            }
        }
        catch (Exception ex)
        {
            ULog.Exception(nameof(TargetPool<T>) + ".Map()", ex, null);

        }
    }

    /// <summary>
    /// Executes the provided update function on all given targetables
    /// </summary>
    /// <param name="envTargets"></param>
    /// <param name="update"></param>
    public void UpdateAll(
        IEnumerable<ITargetable> envTargets,
        Action<T, ITargetable> update)
    {
        try
        {
            remove.Clear();
            touched.Clear();
            foreach (var t in envTargets)
            {
                var goid = t.GameObjectInstanceId;
                if (!map.TryGetValue(goid, out var instance))
                {
                    try
                    {
                        instance = Instantiate(t);
                        map.Add(goid, instance);
                    }
                    catch (Exception ex)
                    {
                        ULog.Exception(nameof(TargetPool<T>) + $".Instantiate({t})", ex, null);
                        continue;
                    }
                }
                update(instance, t);
                touched.Add(goid);
            }

            remove.AddRange(map.Where(x => !touched.Contains(x.Key)));

            foreach (var p in remove)
            {
                var instance = p.Value;
                try
                {
                    Destroy(instance, false);
                }
                catch (Exception ex)
                {
                    ULog.Exception(nameof(TargetPool<T>) + ".Flush(" + p.Key + ")", ex, null);
                }
                finally
                {
                    map.Remove(p.Key);
                }
            }
        }
        catch (Exception ex)
        {
            ULog.Exception(nameof(TargetPool<T>) + ".UpdateAllWithExtra()", ex, null);

        }
    }
}