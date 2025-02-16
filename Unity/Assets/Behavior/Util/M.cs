using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class M
{
    public static float Sqrt2 { get; } = 1.4142135623730950488016887242097f;

    public static Vector2 FlatNormalized(Vector3 source) => Flat(source).normalized;
    public static Vector2 Flat(Vector3 source) => new Vector2(source.x, source.z);

    public static Vector3 UnFlat(Vector2 flat) => new Vector3(flat.x, 0, flat.y);

    public static Vector2 FlatNormal(Vector2 flatAxis) => new Vector2(-flatAxis.y, flatAxis.x);

    public static Vector3 V3(float v) => new Vector3(v, v, v);
    public static Vector3 V3(float x, float y, float z) => new Vector3(x, y, z);

    public static float Saturate(float x) => Mathf.Clamp01(x);
    public static float Interpolate(float a, float b, float x) => a * (1f -x) + b*x;
    public static float Sqr(float x) => x * x;
    public static float Abs(float x) => Mathf.Abs(x);
    public static float Max(float x, float y) => Mathf.Max(x, y);
    public static float Min(float x, float y) => Mathf.Min(x, y);
    /// <summary>
    /// Interpolates smoothly from 0 to 1 based on x compared to a and b.
    /// https://developer.download.nvidia.com/cg/smoothstep.html
    /// </summary>
    /// <param name="a">Minimum reference value(s)</param>
    /// <param name="b">Maximum reference value(s)</param>
    /// <param name="x">Value to compute from</param>
    /// <returns>Interpolated value in [0,1]</returns>
    public static float Smoothstep(float a, float b, float x)
    {
        float t = Saturate((x - a) / (b - a));
        return t * t * (3f - (2f * t));
    }

    public static float Dot(Vector3 right, Vector3 delta)
        => Vector3.Dot(right, delta);
}
