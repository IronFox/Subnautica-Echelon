using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Euler angles with a locked Z component. Vertical rotation (around the X axis) is limited to [MinX,MaxX],
/// horizontal (around the Y axis) is limited to [0,360]
/// </summary>
public readonly struct LockedEuler
{
    /// <summary>
    /// Rotation around the X axis in [MinX, MaxX]
    /// </summary>
    public float X { get; }
    /// <summary>
    /// Rotation around the Y axis in [0,360]
    /// </summary>
    public float Y { get; }

    public TransformLocality Locality { get; }

    public const float MinX = -88f;
    public const float MaxX = 88f;

    public override string ToString() => $"({X},{Y})";

    public LockedEuler(float x, float y, TransformLocality locality)
    {
        X = x;
        Y = y;
        Locality = locality;
    }
    private static float SanitizeX(float x)
    {
        if (x == float.NaN)
            return 0;
        if (x == float.PositiveInfinity)
            return MaxX;
        if (x == float.NegativeInfinity)
            return MinX;
        while (x > 180)
            x -= 360;
        while (x < -180)
            x += 360;
        return Mathf.Clamp(x, MinX, MaxX);
    }

    private static float SanitizeY(float y)
    {
        if (y == float.NaN || y == float.PositiveInfinity || y == float.NegativeInfinity)
            return 0;
        return Mathf.Repeat(y, 360);
    }

    public LockedEuler RotateBy(float x, float y)
    {
        return new LockedEuler(SanitizeX(x + X), SanitizeY(y + Y), Locality);
    }

    public LockedEuler RotateBy(float x, float y, float factor)
        => RotateBy(x * factor, y * factor);

    public void ApplyTo(Transform target)
    {
        switch (Locality)
        {
            case TransformLocality.Global:
                target.eulerAngles = Vector;
                break;
            case TransformLocality.Local:
                target.localEulerAngles = Vector;
                break;
        }
    }
    public static LockedEuler FromAngles(Vector3 e, TransformLocality locality)
    {
        
        return new LockedEuler(SanitizeX(e.x), SanitizeY(e.y), locality);

    }
    public static LockedEuler FromGlobal(Transform target)
        => FromAngles(target.eulerAngles, TransformLocality.Global);
    public static LockedEuler FromLocal(Transform target)
        => FromAngles(target.localEulerAngles, TransformLocality.Local);
    public static LockedEuler From(Quaternion q, TransformLocality locality)
        => FromAngles(q.eulerAngles, locality);

    public Vector3 Forward          => Quaternion * Vector3.forward;
    public Vector3 Right            => Quaternion * Vector3.right;
    public Vector3 Up               => Quaternion * Vector3.up;
    public Quaternion Quaternion    => Quaternion.Euler(X, Y, 0);
    public Vector3 Vector           => new Vector3(X, Y, 0);

    public static LockedEuler Slerp(LockedEuler x, LockedEuler y, float t)
        => From(Quaternion.Slerp(x.Quaternion, y.Quaternion, t), x.Locality);


    public static LockedEuler FromForward(Vector3 forward, TransformLocality locality)
        => FromAngles(Quaternion.FromToRotation(Vector3.forward, forward).eulerAngles, locality);
};