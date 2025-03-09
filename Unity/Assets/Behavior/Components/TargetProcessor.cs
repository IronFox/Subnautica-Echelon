using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TargetProcessor : MonoBehaviour
{
    private readonly TargetEnvironment targetEnvironment = new TargetEnvironment();
    private readonly ReadOnlyTargetEnvironment latest = new ReadOnlyTargetEnvironment();
    private Transform cameraSpace;
    private EchelonControl echelon;
    private UpdateProcess process;

    private bool work = true;

    public bool Work
    {
        get { return work; } set
        {
            if (work == value) return;
            work = value;
            enabled = work;
            if (!work)
                latest.Purge();
        }

    }



    // Start is called before the first frame update
    void Start()
    {
        echelon = GetComponent<EchelonControl>();
        cameraSpace = echelon.trailSpace;
    }

    public ReadOnlyTargetEnvironment Latest => latest;
    
    // Update is called once per frame
    void Update()
    {
        if (process != null)
        {
            if (!process.Next(TimeSpan.FromMilliseconds(5)))
            {
                latest.Update(targetEnvironment);
                process = null;
                TargetListeners.Of(echelon, cameraSpace).SignalNewEnvironment(Latest);
            }
        }
        else
            process = targetEnvironment.Update(transform.position, 500, transform, cameraSpace);

    }
}


public class ReadOnlyTargetEnvironment
{
    public ReadOnlyTargetEnvironment()
    {
    }

    private Vector3 sensorCenter;
    private readonly List<AdapterTargetable> targets = new List<AdapterTargetable>();
    private readonly HashSet<int> objectInstanceIds = new HashSet<int>();

    public IReadOnlyList<AdapterTargetable> Targets => targets;
    public Vector3 SensorCenter => sensorCenter;

    public bool IsTarget(int objectInstanceId)
    {
        if (objectInstanceIds.Count == 0 && targets.Count > 0)
            foreach (var target in targets)
                objectInstanceIds.Add(target.GameObjectInstanceId);
        return objectInstanceIds.Contains(objectInstanceId);
    }

    internal void Update(TargetEnvironment source)
    {
        targets.Clear();
        targets.AddRange(source.Targets);
        objectInstanceIds.Clear();
        sensorCenter = source.SensorCenter;
    }

    internal void Purge()
    {
        targets.Clear();
        objectInstanceIds.Clear();
    }
}