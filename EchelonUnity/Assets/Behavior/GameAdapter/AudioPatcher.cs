using System;
using UnityEngine;

public static class AudioPatcher
{
    public static void Patch(AudioSource source)
    {
        if (Patcher != null)
        {
            Patcher(source);
        }
        //else
        //    ULog.Fail($"Patcher not configured. Cannot patch audio source {source.name}");
    }

    public static void PatchAll(Transform transform)
    {
        if (Patcher == null)
        {
            ULog.Fail($"Patcher not configured. Cannot patch audio sources in {transform.name}");
            return;
        }
        var sources = transform.GetComponentsInChildren<AudioSource>();
        foreach (var source in sources)
            Patcher(source);
    }

    public static Action<AudioSource> Patcher { get; set; }

}
