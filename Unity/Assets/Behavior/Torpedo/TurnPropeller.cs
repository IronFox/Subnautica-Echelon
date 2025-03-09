using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnPropeller : PerformanceCaptured_U
{
    public Transform propeller;
    public float rps = -720;
    public float speedScale = 1;
    private float angle;
    private SoundAdapter aa;
    private ParticleSystem ps;
    private float baseRate, baseSpeed;
    // Start is called before the first frame update
    void Start()
    {
        aa = GetComponent<SoundAdapter>();
        ps = GetComponentInChildren<ParticleSystem>();
        baseRate = ps.emission.rateOverTimeMultiplier;
        baseSpeed = ps.main.startSpeedMultiplier;
    }

    // Update is called once per frame
    protected override void P_Update()
    {
        if (float.IsNaN(rps))
            return;
        if (float.IsNaN(speedScale))
            speedScale = 1;
        angle = Mathf.Repeat(angle + Time.deltaTime * rps * speedScale, 360f);
        if (propeller != null)
            propeller.localEulerAngles = M.V3(0, 0, angle);

        aa.pitch = 0.5f + speedScale;
        aa.volume = speedScale;

        var em = ps.emission;
        em.enabled = speedScale > 0.1f;
        em.rateOverTimeMultiplier = baseRate * speedScale;

        var main = ps.main;
        main.startSpeedMultiplier = baseSpeed * speedScale;
    }
}
