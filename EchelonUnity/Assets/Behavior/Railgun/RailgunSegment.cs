using UnityEngine;

public class RailgunSegment : MonoBehaviour
{
    public ParticleSystem particles;
    public float length = 1;
    public float radius = 0.1f;
    private float lifetime;
    //private Color color;
    private float intensity;
    public Light emissiveLight;
    public float maxLifetime = 5;
    // Start is called before the first frame update
    void Start()
    {
        //cylinder.transform.localScale = M.V3(radius, length, radius);
        var shape = particles.shape;
        shape.scale = M.V3(radius * 5, length * 2, radius * 5);
        //color = cylinder.materials[0].GetColor($"_Color");
        intensity = emissiveLight.intensity;
        //var emission = particles.emission;
        //emission.rateOverTimeMultiplier = length;
    }

    // Update is called once per frame
    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime > maxLifetime)
            Destroy(gameObject);
        else
        {
            var brightness = M.Smoothstep(0, 0.02f, lifetime) * (1f - M.Smoothstep(0.1f, maxLifetime / 2, lifetime));
            //cylinder.materials[0].SetColor($"_Color", M.C(color, brightness * color.a));

            var emission = particles.emission;
            emission.rateOverTimeMultiplier = 100f * brightness;

            emissiveLight.intensity = intensity * brightness;

        }

    }
}
