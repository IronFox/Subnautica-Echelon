using System;
using UnityEngine;

public static class ExplosionAdapter
{
    public static Action<(GameObject ExplosionObject, Vector3 Center, float Radius, float Magnitude)> HandleExplosion { get; set; } = exp => { }; 
}