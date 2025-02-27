using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PositionCamera : MonoBehaviour
{
    // Start is called before the first frame update

    public BoxCollider referenceBoundingBox;
    public Rigidbody subRoot;
    private float distanceToTarget;
    private Transform target;
    private float minDistanceToTarget;
    private float maxDistanceToTarget;
    public bool positionBelowTarget;
    public Collider shipCollider;

    private float verticalOffset;

    private float h = 0;
    public float zoomAxis;

    private TargetScanner scanner;

    public float DistanceToTarget => distanceToTarget;

    void Start()
    {
        scanner= GetComponentInChildren<TargetScanner>();
        target = subRoot.transform;
        distanceToTarget = Vector3.Distance(referenceBoundingBox.transform.position, transform.transform.position);
        minDistanceToTarget = referenceBoundingBox.size.magnitude;
        maxDistanceToTarget = minDistanceToTarget * 5;
        ConsoleControl.Write($"Valid 3rd person camera distance range is [{minDistanceToTarget},{maxDistanceToTarget}]");
        distanceToTarget = Mathf.Clamp( distanceToTarget, minDistanceToTarget, maxDistanceToTarget );
        ConsoleControl.Write($"3rd camera distance set to {distanceToTarget}");
        verticalOffset = 
            referenceBoundingBox.size.y * referenceBoundingBox.transform.localScale.y * 1.5f;
    }

    private string loggedCollider;

    void LateUpdate()
    {
        distanceToTarget *= Mathf.Pow(1.5f, zoomAxis);
        distanceToTarget = Mathf.Clamp(distanceToTarget, minDistanceToTarget, maxDistanceToTarget);

        scanner.minDistance = distanceToTarget;


        var wantH = positionBelowTarget ? -verticalOffset : verticalOffset;

        h += (wantH - h) * 2f * Mathf.Min(Time.deltaTime, 1f);

        var lookAtTarget = target.position + referenceBoundingBox.transform.up * h;

        var wantPosition = lookAtTarget - transform.forward * distanceToTarget;
        Vector3 targetPosition;


        var dir = -transform.forward;
        var hits = Physics.RaycastAll(lookAtTarget, dir, distanceToTarget);

        var dir2 = wantPosition - target.position;
        var dist2 = dir2.magnitude;
        dir2 /= dist2;


        var hits2 = Physics.RaycastAll(target.position, dir2, dist2);


        float closestHit2 = Mathf.Infinity;
        Transform closest2 = null;
        foreach (RaycastHit hit in hits2)
        {
            if (hit.transform.IsChildOf(target)
                || hit.transform.IsChildOf(transform)
                || Physics.GetIgnoreCollision(hit.collider, shipCollider)
                || !hit.collider.enabled
                || hit.collider.isTrigger)
                continue;
            if (hit.distance < closestHit2)
            {
                closest2 = hit.transform;
                closestHit2 = hit.distance;
                if (loggedCollider != hit.transform.name)
                {
                    loggedCollider = hit.transform.name;
                    ConsoleControl.Write("Camera collision with " + hit.transform.name);
                    //HierarchyAnalyzer analyzer = new HierarchyAnalyzer();
                    //analyzer.LogToJson(hit.transform, $@"C:\temp\logs\hit{DateTime.Now:yyyy-MM-dd HH_mm_ss}.json");
                }



            }
        }


        if (closest2 != null)
            targetPosition = target.position + dir2 * Mathf.Max(3f, closestHit2 - 0.5f);
        else
            targetPosition = wantPosition;

        transform.position = targetPosition;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
