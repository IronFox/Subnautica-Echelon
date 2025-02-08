using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PositionCamera : MonoBehaviour
{
    // Start is called before the first frame update

    public BoxCollider targetBoundingBox;
    private float distanceToTarget;
    private Transform target;
    
    public bool positionBelowTarget;

    private float boxHeight;

    private float h = 0;



    void Start()
    {
        target = targetBoundingBox.transform.parent;
        distanceToTarget = Vector3.Distance(targetBoundingBox.transform.position, transform.transform.position);
        boxHeight = targetBoundingBox.size.y * targetBoundingBox.transform.localScale.y;
    }

    

    void LateUpdate()
    {
        var wantH = positionBelowTarget ? -boxHeight : boxHeight;

        h += (wantH - h) * 2f * Mathf.Min(Time.deltaTime, 1f);

        var lookAtTarget = targetBoundingBox.transform.position + targetBoundingBox.transform.up * h;

        var wantPosition = lookAtTarget - transform.forward * distanceToTarget;


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
            if (hit.transform.IsChildOf(target))
                continue;
            if (hit.distance < closestHit2)
            {
                closest2 = hit.transform;
                closestHit2 = hit.distance;
            }
        }

        Vector3 targetPosition;

        if (closest2 != null)
            targetPosition = target.position + dir2 * Mathf.Max(3f ,closestHit2 - 0.5f);
        else
            targetPosition = wantPosition;

        transform.position = targetPosition;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
