using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An attempt at guessing the additional acceleration necessary to compensate the drag
/// at a given travel velocity
/// </summary>
public class DragCompensation
{
    private static readonly Dictionary<float, DragCompensation> map
        = new Dictionary<float, DragCompensation>();

    /// <summary>
    /// Retrieves a drag compensation for a given travel velocity.
    /// If one already exists, that one is reused, otherwise a new one is created
    /// </summary>
    /// <param name="travelVelocity">Target dravel velocity</param>
    /// <returns>Drag compensation calculator</returns>
    public static DragCompensation For(float travelVelocity)
    {
        if (!map.TryGetValue(travelVelocity, out var v))
        {
            v = new DragCompensation();
            v.errorCorrection = travelVelocity * travelVelocity * 0.5f;
            map.Add(travelVelocity, v);
        }
        return v;
    }

    private float errorCorrection = 1f;
    /// <summary>
    /// Updates the error correction delta and returns the final acceleration
    /// </summary>
    /// <param name="needMoreSpeedPerSecond">The net difference between the current
    /// and the desired vehicle velocity in meters per second.
    /// Positive if the vehicle needs to go faster, negative if it needs to go slower</param>
    /// <param name="timeDelta">The time in seconds that passed since the last computation</param>
    /// <returns>Resulting acceleration in m/s²</returns>
    public float Update(float needMoreSpeedPerSecond, float timeDelta)
    {
        var bs = Mathf.Pow(1.1f, needMoreSpeedPerSecond);
        errorCorrection *= Mathf.Pow(bs, timeDelta);
        return needMoreSpeedPerSecond * 10f + errorCorrection;
    }
}
