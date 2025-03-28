
using System;
using System.Linq;
using UnityEngine;

public static class PhysicsHelper
{
    public static bool IsExcluded(Collider incomingCollider, params Rigidbody[] ignoreCollisionsWithIncomingBodies)
        => incomingCollider.attachedRigidbody != null && ignoreCollisionsWithIncomingBodies.Length > 0 && ignoreCollisionsWithIncomingBodies.Any(x => x != null && incomingCollider.attachedRigidbody == x);

    public static bool CanCollide(Collider incomingCollider, Collider myCollider, params Rigidbody[] ignoreCollisionsWithIncomingBodies)
        => !IsExcluded(incomingCollider, ignoreCollisionsWithIncomingBodies)
        && CanCollide(incomingCollider, myCollider, true);

    public static bool CanCollide(Collider incomingCollider, Collider myCollider, bool ignoreIfSelfIsTrigger)
        => !incomingCollider.isTrigger 
        && incomingCollider.enabled 
        && (
            !myCollider.isTrigger
         || ignoreIfSelfIsTrigger
        ) 
        && !Physics.GetIgnoreCollision(incomingCollider, myCollider);
}