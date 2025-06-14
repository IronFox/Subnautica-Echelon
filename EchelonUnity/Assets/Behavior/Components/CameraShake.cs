using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static float GlobalScale { get; set; } = 1.0f;
    public static float BoostScale { get; set; } = 1f;

    public EchelonControl echelon;

    public float? overdriveIntensity = null;

    private List<ShakeEvent> shakeEvents = new List<ShakeEvent>();
    private ShakeEvent overdriveEvent;
    internal void SignalRailgunFired(Transform shot, int mark)
    {
        var existing = shakeEvents.FirstOrDefault(
            x => x.Target == shot
            && x.Origin == Origin.RailgunCharge);
        if (existing != null)
            existing.Terminate();
        shakeEvents.Add(new ShakeEvent(Origin.RailgunFire, shot, 0.01f, maxDistance: null)
        {
            frequency = 15,
            intensity = mark * mark * 0.25f
        });
    }

    public void ClearRailgunCharging(Transform shot, int mark)
    {
        foreach (var ev in shakeEvents.Where(x => x.Target == shot))
            ev.Terminate();
    }
    public void SignalRailgunCharging(Transform shot, int mark)
    {
        if (!shakeEvents.Any(x => x.Target == shot))
        {
            shakeEvents.Add(new ShakeEvent(Origin.RailgunCharge, shot, 1f, infiniteAge: true, maxDistance: null)
            {
                frequency = 10,
                intensity = mark * 0.03f,
            });
        }
    }

    public void SignalTorpedoFired()
    {
        shakeEvents.Add(new ShakeEvent(Origin.TorpedoFire, echelon.transform, 0.1f, maxDistance: null)
        {
            intensity = 0.06f,
        });
    }


    private static float ExplosionD(float distance)
    {
        return 1f / (1f + 0.01f * distance);
    }
    internal void SignalExplosionStart(Transform explosion, float explosionRadius)
    {
        var distance = M.Distance(transform.parent.position, explosion.position);
        var maxDistance = explosionRadius * 10;
        if (distance > maxDistance)
            return;
        //var proximity = (ExplosionD(distance) - ExplosionD(maxDistance)) / (ExplosionD(0) - ExplosionD(maxDistance));
        shakeEvents.Add(new ShakeEvent(Origin.Explosion, explosion, 0.01f, maxDistance, timeScale: 0.7f)
        {
            //radius = maxDistance,
            frequency = 20,
            intensity = 0.8f,
            //maxAge = 1.5f * proximity,
        });
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (overdriveIntensity.HasValue && BoostScale > 0)
        {
            if (overdriveEvent == null)
            {
                overdriveEvent = new ShakeEvent(Origin.Overdrive, echelon.transform, fadeIn: 2f, maxDistance: null, infiniteAge: true)
                {
                    frequency = 5 * overdriveIntensity.Value,
                    intensity = 0.02f * BoostScale * overdriveIntensity.Value,
                };
                shakeEvents.Add(overdriveEvent);
            }
        }
        else
        {
            if (overdriveEvent != null)
            {
                overdriveEvent.Terminate();
                overdriveEvent = null;
            }
        }


        Vector2 intensity = Vector3.zero;
        foreach (var ev in shakeEvents)
        {
            ev.Advance();
            if (!ev.Target)
            {
                ev.Log("Target expired");
                ev.Terminate();
            }
            intensity += ev.GetShake(transform.parent.position);
        }
        foreach (var ev in shakeEvents.Where(x => x.ShouldRemove).ToList())
        {
            ev.Log("Removing");
            shakeEvents.Remove(ev);
        }
        intensity *= GlobalScale;

        transform.localPosition = intensity;
    }

    internal void SignalCollision(Collision collision)
    {
        shakeEvents.Add(new ShakeEvent(Origin.Collision, echelon.transform, fadeIn: 0.01f, maxDistance: 20)
        {
            intensity = collision.impulse.magnitude * 0.0000075f,
            frequency = 20,
        });
    }

    enum Origin
    {
        RailgunCharge,
        RailgunFire,
        TorpedoFire,
        Explosion,
        Overdrive,
        Collision
    }

    class ShakeEvent
    {
        public Origin Origin { get; }
        public float frequency = 20;
        private float scaledAge;
        private float absoluteAge;
        public float intensity = 1;
        private float? maxDistance;
        //public float radius = 100;
        //public float maxAge = 1;
        private bool terminal;
        private float terminalSince;
        public float FadeIn { get; }
        public bool InfiniteDistance => !maxDistance.HasValue;
        public bool InfiniteAge { get; }
        public float TimeScale { get; }
        public Transform Target { get; }
        private Vector3 lastLocation;
        private Vector2 _Seed = Random.insideUnitCircle * 10;

        public bool ShouldRemove => scaledAge > 1
            || (terminal && absoluteAge > terminalSince + 1);


        private static float TimeFalloff(float x)
        {
            return 1f / (1f + 10f * x);
        }
        private float Falloff => (TimeFalloff(scaledAge) - TimeFalloff(1)) / (TimeFalloff(0) - TimeFalloff(1));
        private static float DistanceFalloff(float x)
        {
            return 1f / (1f + 0.01f * x * x);
        }
        private float DFalloff => (DistanceFalloff(Distance) - DistanceFalloff(maxDistance.Value)) / (DistanceFalloff(0) - DistanceFalloff(maxDistance.Value));

        private float Distance => M.Distance(lastLocation, LastCamera);
        public Vector3 LastCamera { get; private set; }

        public void Log(string message)
        {
            //Debug.Log($"[{Origin}]<{Target}>@{age} {message} ");
        }

        public void Advance()
        {
            absoluteAge += Time.deltaTime;
            if (!InfiniteAge && !InfiniteDistance)
            {
                float scale = 0.05f / (DFalloff * this.intensity);
                scaledAge += scale * Time.deltaTime * TimeScale;
            }
            else if (!InfiniteAge)
                scaledAge += Time.deltaTime * TimeScale;
        }

        public void Terminate()
        {
            if (!terminal)
            {
                terminal = true;
                terminalSince = absoluteAge;
                Log("Terminating");
            }
        }
        public ShakeEvent(Origin type, Transform target, float fadeIn, float? maxDistance, bool infiniteAge = false, float timeScale = 1)
        {
            Origin = type;
            Target = target;
            FadeIn = fadeIn;
            this.maxDistance = maxDistance;
            InfiniteAge = infiniteAge;
            TimeScale = timeScale;
            Log("Created");
        }
        public Vector2 GetShake(Vector3 camera)
        {
            if (Target)
                lastLocation = Target.position;
            LastCamera = camera;
            var magnitude =
                Falloff
                * M.Smoothstep(0, FadeIn, absoluteAge)
                * intensity;

            if (maxDistance.HasValue)
                magnitude *= (1f - M.Smoothstep(0, maxDistance.Value, M.Distance(lastLocation, camera)));


            if (terminal)
                magnitude *= 1f - M.Smoothstep(0, 1, (absoluteAge - terminalSince));

            var x = Mathf.Sin(frequency * (_Seed.x + _Seed.y + Time.time));
            var y = Mathf.Cos(frequency * (_Seed.x * Mathf.PI + _Seed.y * 2.132f + Time.time * 1.0234f));

            var time = absoluteAge;
            var noise = new Vector2(
                    Mathf.PerlinNoise(_Seed.x, time * 0.5f) - 0.5f,
                    Mathf.PerlinNoise(_Seed.y, time * 0.5f) - 0.5f
                    );
            var direction = magnitude
                * noise;
            return magnitude * new Vector2(x + noise.x, y + noise.y);

        }
    }

}
