using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEnabledWhileBoarded : CommonBoardingListener
{
    private Light light;
    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
    }

    public override void SignalOnboardingBegin()
    {
        if (light != null)
            light.enabled = true;
    }

    public override void SignalOffBoardingBegin()
    {
        if (light != null)
            light.enabled = false;
    }

}
