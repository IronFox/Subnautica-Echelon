using FMOD;
using Nautilus.Utility;
using Subnautica_Echelon.Logs;
using System;
using System.Collections.Generic;

namespace Subnautica_Echelon.Adapters
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
                float range = cfg.MaxDistance - cfg.MinDistance;
                for (int ix = 0; ix <= 10; ix++)
                {
                    float distance = Sqr((float)ix / 10) * range + cfg.MinDistance;
                    float worldDistance = distance;

                    distance /= halfDistance / M.Sqrt2;
                    //Log.Write($"Distance modified by halfDistance({halfDistance}): {distance}");

                    float volume = M.Saturate(1f / (distance * distance) - 1f / (cfg.MaxDistance * cfg.MaxDistance));
                    rolloff.Add(new VECTOR
                    {
                        x = worldDistance,
                        y = volume
                    });
                    //Log.Write($"Rolloff added: {worldDistance},{volume}");
                }
                var rolloffArray = rolloff.ToArray();



                Check($"sound.set3DCustomRolloff(ref rolloffArray[0], {rolloffArray.Length})", sound.set3DCustomRolloff(ref rolloffArray[0], rolloffArray.Length));
                Check($"sound.set3DMinMaxDistance({cfg.MinDistance}, {cfg.MaxDistance})", sound.set3DMinMaxDistance(cfg.MinDistance, cfg.MaxDistance));




                if (!AudioUtils.TryPlaySound(sound, "bus:/master", out var channel))
                    throw new InvalidOperationException($"AudioUtils.TryPlaySound(sound, \"bus:/master\", out var channel) failed");

                Check($"Channel.setVolume({cfg.Volume})", channel.setVolume(0));
                Check($"Channel.setPitch({cfg.Pitch})", channel.setPitch(0.01f));
                Check($"Channel.set3DMinMaxDistance({cfg.MinDistance}, {cfg.MaxDistance})", channel.set3DMinMaxDistance(cfg.MinDistance, cfg.MaxDistance));




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

}