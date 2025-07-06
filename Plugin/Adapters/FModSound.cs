using FMOD;
using System;
using UnityEngine;

namespace Subnautica_Echelon.Adapters
{

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
                UnityEngine.Object.Destroy(Component);
            }
            catch (Exception ex)
            {
                PLog.Exception($"FModSound.Dispose()", ex, null);
            }

        }
    }
}
