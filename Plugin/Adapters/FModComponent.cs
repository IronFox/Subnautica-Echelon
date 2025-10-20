using UnityEngine;

namespace Subnautica_Echelon.Adapters
{

    internal class FModComponent : MonoBehaviour
    {
        public FModSound? sound;

        public void OnDestroy()
        {
            PLog.Write($"Disposing FModComponent ({sound?.Channel.handle})");
            sound?.Dispose();
        }

        public void Update()
        {
            if (sound is null || !sound.Update(Time.deltaTime))
            {
                PLog.Fail($"FModComponent.sound({sound?.Channel.handle}).Update() returned false. Self-destructing");
                sound = null;//there is something going on in this case. better just unset and don't touch it
                Destroy(this);
            }
        }
    }

}
