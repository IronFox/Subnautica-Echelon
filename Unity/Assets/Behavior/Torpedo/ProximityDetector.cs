using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityDetector : PerformanceCaptured_U
{
    public Collider regularCollider;
    public Detonator detonator;
    public Rigidbody doNotCollideWith;
    public TargetPredictor targetPredictor;
    public float targetTriggerDistance = 0.5f;
    private int isIntersectingWithExclusionCount;

    public bool IsIntersectingWithExclusion => isIntersectingWithExclusionCount > 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider collider)
    {
        bool excluded = PhysicsHelper.IsExcluded(collider, doNotCollideWith);
        if (excluded)
            isIntersectingWithExclusionCount++;
        if (excluded
            || collider.attachedRigidbody == null
            || !PhysicsHelper.CanCollide(collider, regularCollider, true))
        {
            //ConsoleControl.Write($"Cannot react to proximity of {collider.name} (excluded={excluded},nobody={collider.attachedRigidbody == null}, other={!PhysicsHelper.CanCollide(collider, regularCollider, true)}). Ignoring");
            return;
        }
        var rb = collider.attachedRigidbody;
        var rbName = rb != null ? rb.name : "<null>";
        ConsoleControl.Write($"Detected proximity of {collider.name} attached to {rbName}. Exclusion is set to {doNotCollideWith}. Detonating");

        detonator.Detonate();
    }

    private void OnTriggerExit(Collider collider)
    {
        bool excluded = doNotCollideWith != null && collider.attachedRigidbody == doNotCollideWith;
        if (excluded)
            isIntersectingWithExclusionCount--;

    }

    // Update is called once per frame
    protected override void P_Update()
    {
        if (targetPredictor != null)
        {
            var prediction = targetPredictor.target;
            if (prediction != null && prediction.Exists)
            {
                var dist = M.Distance(prediction.Position, transform.position)
                    //- prediction.GlobalSize.magnitude
                    ;
                if (dist < targetTriggerDistance)
                {
                    ConsoleControl.Write($"Detected distance touch with target");
                    detonator.Detonate();
                }
                //else
                //    Debug.Log(dist);
            }

        }
    }
}
