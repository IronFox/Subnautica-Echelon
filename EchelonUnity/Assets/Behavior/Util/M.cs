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
    public static Vector4 V4(float v) => new Vector4(v, v, v, v);
    public static Vector4 V4(Vector3 v, float w) => new Vector4(v.x, v.y, v.z, w);
    public static Vector4 V4(float x, float y, float z, float w) => new Vector4(x, y, z, w);

    public static float Saturate(float x) => Mathf.Clamp01(x);
    public static float Interpolate(float a, float b, float x) => a * (1f -x) + b*x;
    public static Vector3 Interpolate(Vector3 a, Vector3 b, float x) => a * (1f -x) + b*x;
    public static float Sqr(float x) => x * x;
    public static float Sqr(Vector3 x) => Vector3.Dot(x, x);
    public static float Abs(float x) => Mathf.Abs(x);
    public static float Max(float x, float y) => Mathf.Max(x, y);
    public static float Max(float x, float y, float z) => Mathf.Max(x, y, z);
    public static Vector3 Max(Vector3 x, float y) => Max(x,V3(y));
    public static float Min(float x, float y) => Mathf.Min(x, y);
    public static Vector3 Max(Vector3 a, Vector3 b)
        => new Vector3(Max(a.x, b.x), Max(a.y, b.y), Max(a.z, b.z));
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

    public static float LinearStep(float a, float b, float x)
    {
        return (x - a) / (b - a);
    }

    public static float Dot(Vector3 right, Vector3 delta)
        => Vector3.Dot(right, delta);

    public static RayDistance Distance(Ray ray, Vector3 point)
    {
        var d = point - ray.origin;
        var along = M.Dot(ray.direction, d);
        var onRay = ray.GetPoint(along);
        var cross = point - onRay;
        return new RayDistance(along, cross.magnitude);
    }
    public static float Distance(Vector3 a, Vector3 b)
        => Vector3.Distance(a, b);
    public static float SqrDistance(Vector3 a, Vector3 b)
        => Sqr(a.x - b.x) + Sqr(a.y - b.y) + Sqr(a.z - b.z);

    public static float DegToRad(float deg)
        => deg * Mathf.Deg2Rad;
    public static float RadToDeg(float rad)
        => rad * Mathf.Rad2Deg;


    public static QuadraticSolution SolveQuadraticEquation(float a, float b, float c)
    {
        if (Mathf.Abs(a) < Mathf.Epsilon)
        {
            //0 = b * x + c
            //x = -c / b
            if (Mathf.Abs(b) < Mathf.Epsilon)
                return default;
            return QuadraticSolution.One(-c / b);
        }


        var root = b*b -4*a*c;
        if (root < 0)
            return default;
        var a2 = a * 2;
        if (root <= Mathf.Epsilon)
            return QuadraticSolution.One(-b / a2);

        root = Mathf.Sqrt(root);
        var x0 = (-b - root) / a2;
        var x1 = (-b + root) / a2;
        return QuadraticSolution.Two(x0, x1);
    }

    public static float Round(float velocitv, int digits)
    {
        float scale = Mathf.Pow(10, digits);
        return Mathf.Round(velocitv * scale) / scale;
    }

    public static Vector2 V2(float v) => new Vector2(v, v);


}


public readonly struct QuadraticSolution
{
    public float? X0 { get; }
    public float? X1 { get; }

    public bool HasAnySolution => X0.HasValue;

    private static bool IsNonNegative(float?x)
        => x.HasValue && x.Value >= 0;

    public float? SmallestNonNegative =>
        IsNonNegative(X0)
            ? IsNonNegative(X1)
                ? Mathf.Min(X1.Value, X1.Value)
                : X0.Value
            : IsNonNegative(X1)
                ? X1.Value
                : (float?) null;


    private QuadraticSolution(float? x0, float? x1)
    {
        X0 = x0;
        X1 = x1;
    }

    public static QuadraticSolution Two(float x0, float x1)
        => new QuadraticSolution(x0, x1);

    public static QuadraticSolution One(float x)
    {
        return new QuadraticSolution(x, null);
    }




}


public readonly struct RayDistance
{
    public float DistanceAlongRay { get; }
    public float DistanceToClosesPointOnRay { get; }

    public RayDistance(float distanceAlongRay, float distanceToClosesPointOnRay)
    {
        DistanceAlongRay = distanceAlongRay;
        DistanceToClosesPointOnRay = distanceToClosesPointOnRay;
    }
}