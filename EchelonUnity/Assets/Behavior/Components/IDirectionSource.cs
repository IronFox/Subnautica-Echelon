using UnityEngine;

public interface IDirectionSource
{
    Vector3 Forward { get; }
    Vector3 Right { get; }
    Vector3 Up { get; }

    /// <summary>
    /// Weight of this direction source. Lower values decrease the alignment correction.
    /// 0 means no alignment. Max 1
    /// </summary>
    float Impact { get; }

    /// <summary>
    /// Weight of this direction source aling the Z axis. Lower values decrease the z alignment correction.
    /// 0 means no alignment along the Z axis. Max 1
    /// </summary>
    float ZImpact { get; }
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

    public float Impact { get; }  = 1.0f;
    public float ZImpact { get; }  = 1.0f;
}
