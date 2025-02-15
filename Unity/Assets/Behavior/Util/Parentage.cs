using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct Parentage
{
    public TransformDescriptor Transform { get; }
    public Transform Parent { get; }
    public Transform Target { get; }

    public Parentage(TransformDescriptor transform, Transform parent, Transform target)
    {
        Transform = transform;
        Parent = parent;
        Target = target;
    }

    public static Parentage FromLocal(Transform t)
        => new Parentage(TransformDescriptor.FromLocal(t), t.parent, t);

    public void Restore()
    {
        if (Target != null)
        {
            Target.parent = Parent;
            Transform.ApplyTo(Target);
        }
    }
}
