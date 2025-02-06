using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class M
{
    public static Vector2 FlatNormalized(Vector3 source) => Flat(source).normalized;
    public static Vector2 Flat(Vector3 source) => new Vector2(source.x, source.z);

    public static Vector3 UnFlat(Vector2 flat) => new Vector3(flat.x, 0, flat.y);

    public static Vector2 FlatNormal(Vector2 flatAxis) => new Vector2(-flatAxis.y, flatAxis.x);
}
