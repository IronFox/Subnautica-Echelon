using UnityEngine;

public class RailgunCharge : MonoBehaviour
{
    public Renderer isotropicSprite;
    public Renderer wideSprite;
    public float t;
    private float unclampedTime;
    public bool doCharge;
    public float chargeSeconds = 2;
    private bool doPulse;
    public float size;
    public SoundAdapter chargeSound;
    public float scale = 10;
    public bool EndReached => doCharge ? t >= 1 : t <= 0;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (doCharge)
        {
            t += Time.deltaTime;

            doPulse = true;
        }
        else
        {
            this.t -= Time.deltaTime * 5;
            doPulse = false;
        }
        float pulseStrength = doPulse ? 1.1f : 1.0f;

        t = M.Clamp(t, 0, chargeSeconds);
        var relative = t / chargeSeconds;
        size = Mathf.Sqrt(relative);
        float pulseFrequency = 5f + relative * 5f;
        float actualSize = size;
        unclampedTime += Time.deltaTime * pulseFrequency;
        if (doPulse)
            size *= (1f + Mathf.Cos(unclampedTime) * 0.1f);
        Scale(isotropicSprite, Vector3.one * scale, size);
        Scale(wideSprite, M.V3(2, 0.1f, 2) * scale, size * 2);
        chargeSound.pitch = 0.5f + relative * 1.5f;
        chargeSound.volume = relative;
    }

    private void Scale(Renderer sprite, Vector3 initialScale, float factor)
    {
        sprite.transform.localScale = initialScale * factor;
    }
}
