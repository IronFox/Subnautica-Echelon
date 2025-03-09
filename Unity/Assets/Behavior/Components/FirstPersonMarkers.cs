using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMarkers : PerformanceCaptured_UL
{
    public bool show;
    public bool firingLeft;
    public bool firingRight;
    public bool overdriveActive;

    public Renderer leftTorpedo;
    public Renderer rightTorpedo;
    public Renderer overdriveLeft;
    public Renderer overdriveRight;

    private bool wasShown = true;
    private Renderer[] childRenderers;
    private HealingLight[] hls;
    // Start is called before the first frame update
    void Start()
    {
        childRenderers = GetComponentsInChildren<Renderer>();
        hls = GetComponentsInChildren<HealingLight>();
    }

    // Update is called once per frame
    protected override void P_Update()
    {
        if (show != wasShown)
        {
            wasShown = show;
            foreach (var renderer in childRenderers)
                renderer.enabled = show;

            foreach (var hl in hls)
                hl.isEnabled = show;
        }


        leftTorpedo.enabled = show && firingLeft;
        rightTorpedo.enabled = show && firingRight;
        overdriveLeft.enabled = show && overdriveActive;
        overdriveRight.enabled = show && overdriveActive;
    }

    protected override void P_LateUpdate()
    {
        if (show)
        {

            var t = CameraUtil.GetTransform(nameof(FirstPersonMarkers));
            if (t != null)
                transform.position = t.position;
        }
    }
}
