using FMOD;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Subnautica_Echelon.MyLogger;

namespace Subnautica_Echelon
{
    internal class FModSoundCreator : ISoundCreator
    {
        public float halfDistance = 10f;
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
                var sound = AudioUtils.CreateSound(cfg.AudioClip,mode
                    );

                

                List<VECTOR> rolloff = new List<VECTOR>();
                float range = (cfg.MaxDistance - cfg.MinDistance);
                for (int ix = 0; ix <= 10; ix++)
                {
                    float distance = Sqr((float)ix / 10) * range + cfg.MinDistance;
                    float worldDistance = distance;

                    distance /= halfDistance / M.Sqrt2;
                    Log.Write($"Distance modified by halfDistance({halfDistance}): {distance}");

                    float volume = M.Saturate(1f / (distance * distance) - (1f / (cfg.MaxDistance * cfg.MaxDistance)));
                    rolloff.Add(new VECTOR
                    {
                        x = worldDistance,
                        y = volume
                    });
                    Log.Write($"Rolloff added: {worldDistance},{volume}");
                }
                var rolloffArray = rolloff.ToArray();

                

                FModSoundCreator.Check($"sound.set3DCustomRolloff(ref rolloffArray[0], {rolloffArray.Length})",sound.set3DCustomRolloff(ref rolloffArray[0], rolloffArray.Length));
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
                Log.Write($"Sound created (isPlaying={isPlaying})");
                var rs = component.sound = new FModSound(cfg, channel, sound, component, rolloffArray);

                return rs;

            }
            catch (Exception ex)
            {
                Log.Write("FModSoundCreator.Instantiate()", ex);
                return null;
            }
        }

        internal static void Check(string action, RESULT result)
        {
            if (result != RESULT.OK)
                throw new InvalidOperationException($"{action} failed with {result}");
        }
    }

    internal class FModComponent : MonoBehaviour
    {
        public FModSound sound;

        public void FixedUpdate()
        {
            sound.Update(Time.fixedDeltaTime);
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

        internal void Update(float timeDelta)
        {
            try
            {
                var position = Component.transform.position;
                var velocity = (position - lastPosition) / timeDelta;
                lastPosition = position;


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



                var result = Channel.set3DAttributes(ref pos, ref vel);
                if (result != RESULT.OK)
                    throw new InvalidOperationException($"Channel.set3DAttributes() failed with {result}");

                Age += timeDelta;
                if (Age > 0.1f && !Recovered)
                {
                    Recovered = true;
                    FModSoundCreator.Check($"Channel.setVolume({Config.Volume})", Channel.setVolume(Config.Volume));
                    FModSoundCreator.Check($"Channel.setPitch({Config.Pitch})", Channel.setPitch(Config.Pitch));
                }
            }
            catch (Exception ex)
            {
                Log.Write($"FModSound.Update({timeDelta})", ex);
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
                FModSoundCreator.Check($"Channel.set3DMinMaxDistance({cfg.MinDistance}, {cfg.MaxDistance})",Channel.set3DMinMaxDistance(cfg.MinDistance, cfg.MaxDistance));
            }
            catch (Exception ex)
            {
                Log.Write($"FModSound.ApplyLiveChanges()",ex);
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
                Log.Write($"FModSound.Dispose()", ex);
            }

        }
    }
}