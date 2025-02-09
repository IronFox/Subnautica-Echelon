using UnityEngine;

public interface IDirectionSource
{
    Vector3 Forward { get; }
    Vector3 Right { get; }
    Vector3 Up { get; }
}

public class TransformDirectionSource : IDirectionSource
{
    public TransformDirectionSource(Transform transform)
    { 
        Transform = transform;
    }

    public Transform Transform { get; }

    public Vector3 Forward => Transform.forward;

    public Vector3 Right => Transform.right;
    public Vector3 Up => Transform.up;
}
