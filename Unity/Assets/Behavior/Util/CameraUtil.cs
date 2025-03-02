

using System;
using System.Collections.Generic;
using UnityEngine;

public static class CameraUtil
{
    public static Transform primaryFallbackCameraTransform;
    public static Transform secondaryFallbackCameraTransform;

    private static Dictionary<string, DateTime> lastSent;
    private static void Warn(string msg)
    {
        var now = DateTime.Now;
        if (!lastSent.TryGetValue(msg, out DateTime dt) || now - dt > TimeSpan.FromSeconds(10))
        {
            lastSent[msg] = now;
            Debug.LogWarning(msg);
        }
    }
    public static Camera GetCamera(string requester)
    {
        var t = GetTransform(requester);
        if (t == null)
            return null;
        var c = t.GetComponent<Camera>();
        if (c == null)
            c = t.GetComponentInChildren<Camera>();
        if (c == null)
            Warn($"Warning: No camera component found in {t}. Returning null to {requester}");
        return c;
    }
    public static Transform GetTransform(string requester)
    {
        if (Camera.main != null)
            return Camera.main.transform;
        Warn("Warning: Camera.main is null");
        if (primaryFallbackCameraTransform == null)
            return primaryFallbackCameraTransform;
        Warn("Warning: Primary fallback camera is null");
        if (secondaryFallbackCameraTransform == null)
            return secondaryFallbackCameraTransform;
        Warn("Warning: Secondary fallback camera is null");

        if (Camera.current != null)
            return Camera.current.transform;
        Warn("Warning: Camera.current is null");

        foreach (var c in Camera.allCameras)
        {
            if (c.isActiveAndEnabled)
            {
                Warn($"Warning: Returning unusual camera {c.name} to {requester}");
                return c.transform;
            }
        }
        Warn($"Warning: No active and enabled camera found. Returning null to {requester}");
        return null;
    }

}