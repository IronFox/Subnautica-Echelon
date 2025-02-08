using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Euler angles with a locked Z component
/// </summary>
public readonly struct LockedEuler
{
    public float X { get; }
    public float Y { get; }


    public override string ToString() => $"({X},{Y})";

    public LockedEuler(float x, float y)
    {
        X = x;
        Y = y;
    }

    public LockedEuler RotateBy(float x, float y)
    {
        return new LockedEuler(Mathf.Clamp(x + X, -88f, 88f), Mathf.Repeat(y + Y, 360));
    }

    public LockedEuler RotateBy(float x, float y, float factor)
        => RotateBy(x * factor, y * factor);

    public void ApplyToGlobal(Transform target)
    {
        target.eulerAngles = Vector;
    }
    public void ApplyToLocal(Transform target)
    {
        target.localEulerAngles = Vector;
    }
    public static LockedEuler FromAngles(Vector3 e)
    {
        float rotX = e.x;
        float rotY = e.y;
        while (rotX > 180)
            rotX -= 360;
        while (rotX < -180)
            rotX += 360;
        return new LockedEuler(rotX, rotY);

    }
    public static LockedEuler FromGlobal(Transform target)
        => FromAngles(target.eulerAngles);
    public static LockedEuler FromLocal(Transform target)
        => FromAngles(target.localEulerAngles);
    public static LockedEuler From(Quaternion q)
        => FromAngles(q.eulerAngles);

    public Quaternion Quaternion
        => Quaternion.Euler(X, Y, 0);
    public Vector3 Vector
        => new Vector3(X, Y, 0);

    public static LockedEuler Slerp(LockedEuler x, LockedEuler y, float t)
        => From(Quaternion.Slerp(x.Quaternion, y.Quaternion, t));
};