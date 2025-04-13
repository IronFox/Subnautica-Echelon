using UnityEngine;

public class ProximityDetector : MonoBehaviour
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
            return;
        }
        var otherControl = collider.attachedRigidbody.gameObject.GetComponent<TorpedoControl>();
        if (otherControl != null)
        {
            return; //prevent torpedoes from blowing each other up
        }
        var rb = collider.attachedRigidbody;
        var rbName = rb != null ? rb.name : "<null>";
        ULog.Write($"Detected proximity of {collider.name} attached to {rbName}. Exclusion is set to {doNotCollideWith}. Detonating");

        detonator.Detonate();
    }

    private void OnTriggerExit(Collider collider)
    {
        bool excluded = doNotCollideWith != null && collider.attachedRigidbody == doNotCollideWith;
        if (excluded)
            isIntersectingWithExclusionCount--;

    }

    // Update is called once per frame
    void Update()
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
                    ULog.Write($"Detected distance touch with target");
                    detonator.Detonate();
                }
            }

        }
    }
}
