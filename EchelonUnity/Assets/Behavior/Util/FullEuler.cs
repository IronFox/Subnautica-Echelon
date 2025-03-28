using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Euler angles
/// </summary>
public readonly struct FullEuler
{
    /// <summary>
    /// Rotation around the X axis
    /// </summary>
    public float X { get; }
    /// <summary>
    /// Rotation around the Y axis
    /// </summary>
    public float Y { get; }
    /// <summary>
    /// Rotation around the Z axis
    /// </summary>
    public float Z { get; }

    public TransformLocality Locality { get; }

    public static FullEuler LocalIdentity { get; } = new FullEuler(0, 0, 0, TransformLocality.Local);
    public static FullEuler GlobalIdentity { get; } = new FullEuler(0, 0, 0, TransformLocality.Global);

    public override string ToString() => $"({X},{Y},{Z})";

    public FullEuler(float x, float y, float z, TransformLocality locality)
    {
        X = x;
        Y = y;
        Z = z;
        Locality = locality;
    }
    private static float SanitizeX(float x)
    {
        if (x == float.NaN)
            return 0;
        if (x == float.PositiveInfinity)
            return 90;
        if (x == float.NegativeInfinity)
            return -90;
        while (x > 180)
            x -= 360;
        while (x < -180)
            x += 360;
        return x;
    }

    private static float SanitizeY(float y)
    {
        if (y == float.NaN || y == float.PositiveInfinity || y == float.NegativeInfinity)
            return 0;
        return Mathf.Repeat(y, 360);
    }

    private static float SanitizeZ(float z)
        => SanitizeX(z);

    public FullEuler RotateBy(float x, float y, float z)
    {
        return new FullEuler(SanitizeX(x + X), SanitizeY(y + Y), SanitizeZ(z + Z), Locality);
    }

    public FullEuler RotateBy(float x, float y, float z, float factor)
        => RotateBy(x * factor, y * factor, z * factor);

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
    public static FullEuler FromAngles(Vector3 e, TransformLocality locality)
    {

        return new FullEuler(SanitizeX(e.x), SanitizeY(e.y), SanitizeZ(e.z), locality);

    }
    public static FullEuler FromGlobal(Transform target)
        => FromAngles(target.eulerAngles, TransformLocality.Global);
    public static FullEuler FromLocal(Transform target)
        => FromAngles(target.localEulerAngles, TransformLocality.Local);
    public static FullEuler From(Quaternion q, TransformLocality locality)
        => FromAngles(q.eulerAngles,locality);

    public Vector3 Forward => Quaternion * Vector3.forward;
    public Vector3 Right => Quaternion * Vector3.right;
    public Vector3 Up => Quaternion * Vector3.up;
    public Quaternion Quaternion => Quaternion.Euler(X, Y, Z);
    public Vector3 Vector => new Vector3(X, Y, Z);

    public static FullEuler Slerp(FullEuler x, FullEuler y, float t)
        => From(Quaternion.Slerp(x.Quaternion, y.Quaternion, t), x.Locality);


    public static FullEuler FromForward(Vector3 forward, TransformLocality locality)
        => FromAngles(Quaternion.FromToRotation(Vector3.forward, forward).eulerAngles, locality);
};