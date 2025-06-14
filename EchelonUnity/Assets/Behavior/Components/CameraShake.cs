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
        shakeEvents.Add(new ShakeEvent(Origin.RailgunFire, shot, 0.01f)
        {
            radius = 100,
            speed = 15,
            intensity = mark * mark * 0.15f,
            maxAge = 1f,
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
            shakeEvents.Add(new ShakeEvent(Origin.RailgunCharge, shot, 1f)
            {
                radius = 100,
                speed = 10,
                intensity = mark * mark * 0.02f,
                maxAge = 100,
            });
        }
    }

    public void SignalTorpedoFired()
    {
        shakeEvents.Add(new ShakeEvent(Origin.TorpedoFire, echelon.transform, 0.1f)
        {
            radius = 100,
            intensity = 0.06f,
            maxAge = 0.5f,
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
        var proximity = (ExplosionD(distance) - ExplosionD(maxDistance)) / (ExplosionD(0) - ExplosionD(maxDistance));
        shakeEvents.Add(new ShakeEvent(Origin.Explosion, explosion, 0.01f)
        {
            radius = maxDistance,
            speed = 20,
            intensity = 0.5f,
            maxAge = 1.5f * proximity,
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
                overdriveEvent = new ShakeEvent(Origin.Overdrive, echelon.transform, 2f)
                {
                    radius = 10000,
                    speed = 5 * overdriveIntensity.Value,
                    intensity = 0.01f * BoostScale * overdriveIntensity.Value,
                    maxAge = 100000
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
            ev.age += Time.deltaTime;
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
        public float speed = 20;
        public float age;
        public float intensity = 1;
        public float radius = 100;
        public float maxAge = 1;
        private bool terminal;
        private float terminalSince;
        public float FadeIn { get; }
        public Transform Target { get; }
        private Vector3 lastLocation;
        private Vector2 _Seed = Random.insideUnitCircle * 10;

        public bool ShouldRemove => age > maxAge
            || (terminal && age > terminalSince + 1);

        public void Log(string message)
        {
            Debug.Log($"[{Origin}]<{Target}>@{age} {message} ");
        }

        public void Terminate()
        {
            if (!terminal)
            {
                terminal = true;
                terminalSince = age;
                Log("Terminating");
            }
        }
        public ShakeEvent(Origin type, Transform target, float fadeIn)
        {
            Origin = type;
            Target = target;
            FadeIn = fadeIn;
            Log("Created");
        }
        public Vector2 GetShake(Vector3 camera)
        {
            if (Target)
                lastLocation = Target.position;

            var magnitude =
                (1f - M.Smoothstep(0, radius, M.Distance(lastLocation, camera)))
                * Mathf.Max(0, 1f - age / maxAge)
                * M.Smoothstep(0, FadeIn, age)
                * intensity;
            if (terminal)
                magnitude *= 1f - M.Smoothstep(0, 1, (age - terminalSince));

            var x = Mathf.Sin(speed * (_Seed.x + _Seed.y + Time.time));
            var y = Mathf.Cos(speed * (_Seed.x * Mathf.PI + _Seed.y * 2.132f + Time.time * 1.0234f));

            var time = Time.time;
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
