using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public readonly struct TransformDescriptor 
{
    public static TransformDescriptor LocalIdentity { get; } = new TransformDescriptor(FullEuler.LocalIdentity, Vector3.zero);
    public static TransformDescriptor GlobalIdentity { get; } = new TransformDescriptor(FullEuler.GlobalIdentity, Vector3.zero);

    public TransformDescriptor(FullEuler rotation, Vector3 position) : this()
    {
        Euler = rotation;
        Position = position;
    }

    public FullEuler Euler { get; }
    public Vector3 Position { get; }


    public static TransformDescriptor FromLocal(Transform source)
        => new TransformDescriptor(FullEuler.FromLocal(source), position: source.localPosition);
    public static TransformDescriptor FromGlobal(Transform source)
        => new TransformDescriptor(FullEuler.FromGlobal(source), position: source.position);

    public void ApplyTo(Transform target)
    {
        Euler.ApplyTo(target);
        switch (Euler.Locality)
        {
            case TransformLocality.Local:
                target.localPosition = Position;
                break;
            case TransformLocality.Global:
                target.position = Position;
                break;
        }
    }

    public static TransformDescriptor Lerp(TransformDescriptor a, TransformDescriptor b, float t)
        => new TransformDescriptor(FullEuler.Slerp(a.Euler, b.Euler, t), Vector3.Lerp(a.Position,b.Position,t));
}
