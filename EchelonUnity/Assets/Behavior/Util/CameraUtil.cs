

using System;
using System.Collections.Generic;
using UnityEngine;

public static class CameraUtil
{
    public static Transform primaryFallbackCameraTransform;
    public static Transform secondaryFallbackCameraTransform;

    private static readonly Dictionary<string, DateTime> lastSent
        = new Dictionary<string, DateTime>();
    private static void Warn(string msg)
    {
        try
        {
            var now = DateTime.Now;
            if (!lastSent.TryGetValue(msg, out DateTime dt) || now - dt > TimeSpan.FromSeconds(10))
            {
                lastSent[msg] = now;
                Debug.LogWarning(msg);
            }
        }
        catch { }   //ignore this one
    }
    public static Camera GetCamera(string requester)
    {
        return Get(requester, true).Camera;
    }
    public static Transform GetTransform(string requester)
    {
        return Get(requester, false).Transform;
    }

    private static Camera GetCameraOf(Transform t)
    {
        var c = t.GetComponent<Camera>();
        if (c == null)
            c = t.GetComponentInChildren<Camera>();
        return c;
    }

    private static (Transform Transform, Camera Camera) FromCamera(Camera camera)
    {
        return (camera.transform, camera);
    }
    private static bool CheckTransform(Transform t, bool requireCamera, out (Transform Transform, Camera Camera) rs)
    {
        if (!requireCamera)
        {
            rs = (t, null);
            return true;
        }

        if (t != null)
        {
            var c = GetCameraOf(t);
            if (c != null)
            {
                rs = (t, c);
                return true;
            }
        }
        rs = (null, null);
        return false;
    }
    private static (Transform Transform, Camera Camera) Get(string requester, bool requireCamera)
    {
        try
        {
            if (Camera.main != null)
                return FromCamera(Camera.main);
            Warn("Warning: Camera.main is null");
            if (CheckTransform(primaryFallbackCameraTransform, requireCamera, out var rs))
                return rs;
            Warn("Warning: Primary fallback camera is null");
            if (CheckTransform(secondaryFallbackCameraTransform, requireCamera, out rs))
                return rs;
            Warn("Warning: Secondary fallback camera is null");

            if (Camera.current != null)
                return FromCamera(Camera.current);
            Warn("Warning: Camera.current is null");

            foreach (var c in Camera.allCameras)
            {
                if (c.isActiveAndEnabled)
                {
                    Warn($"Warning: Returning unusual camera {c.name} to {requester}");
                    return FromCamera(c);
                }
            }
            Warn($"Warning: No active and enabled camera found. Returning null to {requester}");
            return (null, null);
        }
        catch (Exception ex)
        {
            Warn(ex.ToString());
            return (null, null);
        }
    }

}