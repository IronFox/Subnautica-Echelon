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
    public int upgradeLevel = 1;
    /// <summary>
    /// How many times faster discharge is than charge
    /// </summary>
    public const float DischargeSpeedFactor = 5;
    public bool EndReached => doCharge ? t >= chargeSeconds : t <= 0;
    public float EndReachedSeconds { get; private set; }

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
            this.t -= Time.deltaTime * DischargeSpeedFactor;
            doPulse = false;
        }
        float pulseStrength = doPulse ? 1.1f : 1.0f;

        t = M.Clamp(t, 0, chargeSeconds);
        if (EndReached)
            EndReachedSeconds += Time.deltaTime;
        else
            EndReachedSeconds = 0;
        var relative = t / chargeSeconds;
        size = Mathf.Sqrt(relative);
        var upgradeScale = Mathf.Pow(2, upgradeLevel) * 0.5f;
        float pulseFrequency = 5f + relative * 2.5f * upgradeScale;
        unclampedTime += Time.deltaTime * pulseFrequency;
        if (doPulse)
            size *= (1f + Mathf.Cos(unclampedTime) * 0.1f);
        Scale(isotropicSprite, Vector3.one * scale, size);
        Scale(wideSprite, M.V3(2, 0.1f, 2) * scale, size * 2);
        chargeSound.pitch = 0.9f + relative * upgradeScale;
        chargeSound.volume = (M.Saturate(relative * 5) * 0.5f + 0.5f * relative) * Railgun.SoundLevel;
    }

    private void Scale(Renderer sprite, Vector3 initialScale, float factor)
    {
        sprite.transform.localScale = initialScale * factor;
    }
}
