using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEnabledWhileBoarded : CommonBoardingListener
{
    private Light myLight;
    // Start is called before the first frame update
    void Start()
    {
        myLight = GetComponent<Light>();
    }

    public override void SignalOnboardingBegin()
    {
        if (myLight != null)
            myLight.enabled = true;
    }

    public override void SignalOffBoardingBegin()
    {
        if (myLight != null)
            myLight.enabled = false;
    }

}
