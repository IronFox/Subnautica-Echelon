using System;
using UnityEngine;

public static class AudioPatcher
{
    public static void Patch(AudioSource source)
    {
        if (Patcher != null)
        {
            //ConsoleControl.Write($"Patching audio source {source.name}");
            Patcher(source);
        }
        else
            ConsoleControl.Write($"Patcher not configured. Cannot patch audio source {source.name}");
    }

    public static void PatchAll(Transform transform)
    {
        if (Patcher == null)
        {
            ConsoleControl.Write($"Patcher not configured. Cannot patch audio sources in {transform.name}");
            return;
        }
        var sources = transform.GetComponentsInChildren<AudioSource>();
        //ConsoleControl.Write($"Patching {sources.Length} audio sources found in {transform.name}");
        foreach (var source in sources)
            Patcher(source);
    }

    public static Action<AudioSource> Patcher { get; set; }

}
