﻿using FMOD;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Subnautica_Echelon.MyLogger;

namespace Subnautica_Echelon
{
    internal class FModSoundCreator : ISoundCreator
    {
        public float halfDistance = 20f;
        private static float Sqr(float value) => value * value;
        public IInstantiatedSound Instantiate(SoundConfig cfg)
        {

            try
            {
                var mode = MODE.DEFAULT | MODE._3D | MODE.ACCURATETIME
                    //| MODE._3D_INVERSEROLLOFF
                    | MODE._3D_CUSTOMROLLOFF
                    ;
                if (cfg.Loop)
                    mode |= MODE.LOOP_NORMAL;
                else
                    mode |= MODE.LOOP_OFF;
                var sound = AudioUtils.CreateSound(cfg.AudioClip, mode
                    );



                List<VECTOR> rolloff = new List<VECTOR>();
                float range = (cfg.MaxDistance - cfg.MinDistance);
                for (int ix = 0; ix <= 10; ix++)
                {
                    float distance = Sqr((float)ix / 10) * range + cfg.MinDistance;
                    float worldDistance = distance;

                    distance /= halfDistance / M.Sqrt2;
                    //Log.Write($"Distance modified by halfDistance({halfDistance}): {distance}");

                    float volume = M.Saturate(1f / (distance * distance) - (1f / (cfg.MaxDistance * cfg.MaxDistance)));
                    rolloff.Add(new VECTOR
                    {
                        x = worldDistance,
                        y = volume
                    });
                    //Log.Write($"Rolloff added: {worldDistance},{volume}");
                }
                var rolloffArray = rolloff.ToArray();



                FModSoundCreator.Check($"sound.set3DCustomRolloff(ref rolloffArray[0], {rolloffArray.Length})", sound.set3DCustomRolloff(ref rolloffArray[0], rolloffArray.Length));
                FModSoundCreator.Check($"sound.set3DMinMaxDistance({cfg.MinDistance}, {cfg.MaxDistance})", sound.set3DMinMaxDistance(cfg.MinDistance, cfg.MaxDistance));




                if (!AudioUtils.TryPlaySound(sound, "bus:/master", out var channel))
                    throw new InvalidOperationException($"AudioUtils.TryPlaySound(sound, \"bus:/master\", out var channel) failed");

                FModSoundCreator.Check($"Channel.setVolume({cfg.Volume})", channel.setVolume(0));
                FModSoundCreator.Check($"Channel.setPitch({cfg.Pitch})", channel.setPitch(0.01f));
                FModSoundCreator.Check($"Channel.set3DMinMaxDistance({cfg.MinDistance}, {cfg.MaxDistance})", channel.set3DMinMaxDistance(cfg.MinDistance, cfg.MaxDistance));




                var pos = new VECTOR
                {
                    x = cfg.Owner.transform.position.x,
                    y = cfg.Owner.transform.position.y,
                    z = cfg.Owner.transform.position.z
                };

                var vel = new VECTOR
                {
                    x = 0,
                    y = 0,
                    z = 0
                };



                Check($"channel.set3DAttributes(ref pos, ref vel)", channel.set3DAttributes(ref pos, ref vel));

                var component = cfg.Owner.AddComponent<FModComponent>();

                channel.isPlaying(out var isPlaying);
                Log.Write($"Sound ({channel.handle}) created (isPlaying={isPlaying})");
                var rs = component.sound = new FModSound(cfg, channel, sound, component, rolloffArray);

                return rs;

            }
            catch (Exception ex)
            {
                PLog.Exception("FModSoundCreator.Instantiate()", ex, cfg.Owner);
                return null;
            }
        }

        internal static void Check(string action, RESULT result)
        {
            if (result != RESULT.OK)
                throw new FModException($"{action} failed with {result}", result);
        }
    }

    internal class FModComponent : MonoBehaviour
    {
        public FModSound sound;

        public void OnDestroy()
        {
            PLog.Write($"Disposing FModComponent ({sound?.Channel.handle})");
            sound?.Dispose();
        }

        public void Update()
        {
            if (!sound.Update(Time.deltaTime))
            {
                sound = null;//there is something going on in this case. better just unset and don't touch it
                PLog.Fail($"FModComponent.sound({sound.Channel.handle}).Update() returned false. Self-destructing");
                Destroy(this);
            }
        }
    }

    internal class FModSound : IInstantiatedSound
    {
        public SoundConfig Config { get; private set; }
        public FMOD.Channel Channel { get; }
        public FModComponent Component { get; }
        public VECTOR[] RolloffArray { get; }
        public Sound Sound { get; }
        private float Age { get; set; }
        private bool Recovered { get; set; }

        private Vector3 lastPosition;
        public FModSound(SoundConfig config, FMOD.Channel channel, Sound sound, FModComponent component, VECTOR[] rolloffArray)
        {
            Config = config;
            Channel = channel;
            Component = component;
            RolloffArray = rolloffArray;
            Sound = sound;
        }

        internal bool Update(float timeDelta)
        {
            if (timeDelta <= 0)
                return true;
            Vector3 vpos = Vector3.zero, velocity = Vector3.zero;
            try
            {
                var position = Component.transform.position;
                velocity = (position - lastPosition) / timeDelta;
                lastPosition = position;
                vpos = position;

                var pos = new VECTOR
                {
                    x = position.x,
                    y = position.y,
                    z = position.z
                };

                var vel = new VECTOR
                {
                    x = velocity.x,
                    y = velocity.y,
                    z = velocity.z
                };



                FModSoundCreator.Check($"Channel({Channel.handle}).set3DAttributes({position},{velocity})", Channel.set3DAttributes(ref pos, ref vel));

                Age += timeDelta;
                if (Age > 0.1f && !Recovered)
                {
                    Recovered = true;
                    FModSoundCreator.Check($"Channel({Channel.handle}).setVolume({Config.Volume})", Channel.setVolume(Config.Volume));
                    FModSoundCreator.Check($"Channel({Channel.handle}).setPitch({Config.Pitch})", Channel.setPitch(Config.Pitch));
                }
                return true;
            }
            catch (FModException ex)
            {
                PLog.Exception($"FModSound.Update({timeDelta}) [{vpos},{velocity}] ", ex, Config.Owner);
                return ex.Result != RESULT.ERR_INVALID_HANDLE;
            }
            catch (Exception ex)
            {
                PLog.Exception($"FModSound.Update({timeDelta}) [{vpos},{velocity}] ", ex, Config.Owner);
                return true;
            }
        }

        public void ApplyLiveChanges(SoundConfig cfg)
        {
            try
            {
                if (Recovered)
                {
                    FModSoundCreator.Check($"Channel.setVolume({cfg.Volume})", Channel.setVolume(cfg.Volume));
                    FModSoundCreator.Check($"Channel.setPitch({cfg.Pitch})", Channel.setPitch(cfg.Pitch));
                }
                FModSoundCreator.Check($"Channel.set3DMinMaxDistance({cfg.MinDistance}, {cfg.MaxDistance})", Channel.set3DMinMaxDistance(cfg.MinDistance, cfg.MaxDistance));
            }
            catch (Exception ex)
            {
                PLog.Exception($"FModSound.ApplyLiveChanges()", ex, Config.Owner);
            }


            Config = cfg;
        }

        public void Dispose()
        {
            try
            {
                Channel.stop();
                Sound.release();
                GameObject.Destroy(Component);
            }
            catch (Exception ex)
            {
                PLog.Exception($"FModSound.Dispose()", ex, null);
            }

        }
    }
}