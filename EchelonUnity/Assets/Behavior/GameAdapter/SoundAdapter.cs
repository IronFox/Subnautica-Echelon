using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

public class SoundAdapter : MonoBehaviour
{
    public IInstantiatedSound Sound { get; private set; }
    public AudioClip clip;

    public float volume = 1;
    public bool play = true;
    public float minDistance = 1f;
    public float maxDistance = 500f;
    public bool is3D = true;
    public float pitch = 1f;
    public bool loop = false;
    

    void Start()
    {
        
    }

    void OnDestroy()
    {
        Sound?.Dispose();
        Sound = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (clip != null && play)
        {
            var cfg = GetCurrentConfig();

            if (Sound == null || !Sound.Config.IsLiveCompatibleTo(cfg))
            {
                Sound?.Dispose();
                Sound = SoundCreator.Instantiate(cfg);
            }
            else
                if (Sound.Config.IsSignificantlyDifferent(cfg))
                    Sound.ApplyLiveChanges(cfg);
        }
        else if (Sound != null)
        {
            Sound.Dispose();
            Sound = null;
        }
    }

    public void Play()
    {
        if (clip != null)
        {
            var cfg = GetCurrentConfig();
            Sound?.Dispose();
            Sound = SoundCreator.Instantiate(cfg);
        }
    }

    private SoundConfig GetCurrentConfig()
    {
        return new SoundConfig(
            gameObject,
            clip,
            volume: volume,
            minDistance: minDistance,
            maxDistance: maxDistance,
            is3D: is3D,
            pitch: pitch,
            loop: loop
            );
    }


    public static ISoundCreator SoundCreator { get; set; } = new DefaultSoundCreator();
        //new DefaultSoundCreator();

}

public interface IInstantiatedSound : IDisposable
{
    SoundConfig Config { get; }

    void ApplyLiveChanges( SoundConfig cfg );

}

public readonly struct SoundConfig
{
    public GameObject Owner { get; }
    public AudioClip AudioClip { get; }
    public float Pitch { get; }
    public bool Is3D { get; }
    public float Volume { get; }
    public bool Loop { get; }
    public float MinDistance { get; }
    public float MaxDistance { get; }

    public SoundConfig(GameObject owner,
        AudioClip audioClip,
        float volume = 1f,
        float minDistance = 1f,
        float maxDistance = 500f,
        bool loop = false,
        bool is3D = true,
        float pitch = 1f)
    {
        Owner = owner;
        AudioClip = audioClip;
        MinDistance = minDistance;
        MaxDistance = maxDistance;
        Pitch = pitch;
        Is3D = is3D;
        Volume = volume;
        Loop = loop;
    }

    private static bool SigDif(float a, float b)
        => Mathf.Abs(a - b) > 0.005f;
    public bool IsSignificantlyDifferent(SoundConfig other)
    {
        return //!IsLiveCompatibleTo(Other)
            //|| 
            SigDif(Pitch, other.Pitch)
            || SigDif(Volume, other.Volume)
            || SigDif(MinDistance, other.MinDistance)
            || SigDif(MaxDistance, other.MaxDistance)
            ;

    }
    public bool IsLiveCompatibleTo(SoundConfig other)
        => Owner == other.Owner
            && AudioClip == other.AudioClip
            && Loop == other.Loop
        ;
}



public interface ISoundCreator
{
    IInstantiatedSound Instantiate(SoundConfig soundConfig);
}

public class DefaultSoundCreator : ISoundCreator
{
    public IInstantiatedSound Instantiate(SoundConfig soundConfig)
    {
        var source = soundConfig.Owner.AddComponent<AudioSource>();
        source.clip = soundConfig.AudioClip;
        source.playOnAwake = true;
        source.pitch = soundConfig.Pitch;
        source.minDistance = soundConfig.MinDistance;
        source.maxDistance = soundConfig.MaxDistance;
        source.spatialBlend = soundConfig.Is3D ? 1 : 0;
        source.volume = soundConfig.Volume;
        source.loop = soundConfig.Loop;
        AudioPatcher.Patch(source);
        source.Play();

        return new DefaultSound(source, soundConfig);
    }
}

public class EmulatedSpacialSoundCreator : ISoundCreator
{
    public IInstantiatedSound Instantiate(SoundConfig soundConfig)
    {
        var source = soundConfig.Owner.AddComponent<AudioSource>();
        source.clip = soundConfig.AudioClip;
        source.playOnAwake = true;
        source.pitch = soundConfig.Pitch;
        source.minDistance = soundConfig.MinDistance;
        source.maxDistance = soundConfig.MaxDistance;
        source.spatialBlend = 0;
        source.volume = soundConfig.Volume;
        source.loop = soundConfig.Loop;

        var emulator = soundConfig.Owner.AddComponent<SpatialSoundEmulator>();
        emulator.pitch = soundConfig.Pitch;
        emulator.volume = soundConfig.Volume;
        AudioPatcher.Patch(source);
        source.Play();

        return new EmulatedSpacialSound(emulator, source, soundConfig);
    }
}

internal class EmulatedSpacialSound : IInstantiatedSound
{
    public EmulatedSpacialSound(SpatialSoundEmulator emulator, AudioSource source, SoundConfig config)
    {
        Emulator = emulator;
        Source = source;
        Config = config;
    }

    public SpatialSoundEmulator Emulator { get; }
    public AudioSource Source { get; }

    public SoundConfig Config { get; private set; }

    public void ApplyLiveChanges(SoundConfig cfg)
    {
        Source.pitch = cfg.Pitch;
        Source.volume = cfg.Volume;
        Source.maxDistance = cfg.MaxDistance;
        Source.minDistance = cfg.MinDistance;
        Emulator.pitch = cfg.Pitch;
        Emulator.volume = cfg.Volume;

        if (cfg.Volume < 0.01f)
        {
            if (Source.isPlaying)
                Source.Stop();
        }
        else if (!Source.isPlaying)
            Source.Play();

        Config = cfg;

    }

    public void Dispose()
    {
        GameObject.Destroy(Source);
        GameObject.Destroy(Emulator);
    }
}

internal class DefaultSound : IInstantiatedSound
{
    public AudioSource Source { get; }
    public SoundConfig Config { get; private set; }

    public DefaultSound(AudioSource audioSource, SoundConfig config)
    {
        Source = audioSource;
        Config = config;
    }

    public void Dispose()
    {
        GameObject.Destroy( Source );
    }

    public void ApplyLiveChanges(SoundConfig cfg)
    {
        Source.pitch = cfg.Pitch;
        Source.volume = cfg.Volume;
        Source.maxDistance = cfg.MaxDistance;
        Source.minDistance = cfg.MinDistance;
        Source.spatialBlend = cfg.Is3D ? 1 : 0;
        if (cfg.Volume < 0.01f)
        {
            if (Source.isPlaying)
                Source.Stop();
        }
        else if (!Source.isPlaying)
                Source.Play();

        Config = cfg;
    }
}