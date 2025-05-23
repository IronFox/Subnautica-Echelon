﻿using System;
using System.Text;
using TMPro;
using UnityEngine;

public class StatusConsole : CommonBoardingListener
{
    public TextMeshProUGUI statusText;
    public RectTransform parentCanvas;
    private Canvas canvas;

    private string[] status = new string[Enum.GetValues(typeof(StatusProperty)).Length];

    public void SetString(StatusProperty property, string value)
    {
        status[(int)property] = value ?? "<null>";

    }

    public void Set(StatusProperty property, object value)
    {
        status[(int)property] = value?.ToString() ?? "<null>";

    }


    public override void SignalOnboardingBegin()
    {
        var camera = CameraUtil.GetCamera(nameof(StatusConsole));
        if (camera == null)
        {
            ULog.Fail($"Cannot assign camera as worldCamera. Canvas remains off");
            return;
        }
        ULog.Write($"Assigning {camera} as worldCamera of {canvas}");
        if (canvas == null)
            canvas = GetComponent<Canvas>();

        canvas.worldCamera = camera;
        canvas.planeDistance = Mathf.Max(camera.nearClipPlane * 1.1f, 2f);
        ULog.Write($"Set clip plane to distance {canvas.planeDistance}");
        canvas.enabled = true;
        enabled = statusText.enabled = false;
    }

    public override void SignalOffBoardingEnd()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();
        canvas.enabled = false;
        canvas.worldCamera = null;
        enabled = false;
    }

    public void ToggleVisibility()
    {
        enabled = !enabled;
        statusText.enabled = enabled;
        ULog.Write($"Toggled canvas visibility to {enabled}");
    }


    // Start is called before the first frame update
    void Start()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        float w = parentCanvas.rect.width / 2 * 0.9f;
        float h = parentCanvas.rect.height * 0.9f;
        statusText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        statusText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

        statusText.rectTransform.localPosition =
            M.V3(-w + w / 2, 0, 0);


        StringBuilder b = new StringBuilder();
        b.Append("Echelon Status\n");
        for (int i = 0; i < status.Length; i++)
        {
            b.Append(((StatusProperty)i).ToString()).Append(": ")
                .Append(status[i]).Append('\n');
        }

        statusText.text = b.ToString();
    }
}


public enum StatusProperty
{
    Target,
    LeftLauncherTarget,
    RightLauncherTarget,
    LeftLauncherProgress,
    RightLauncherProgress,
    EnergyLevel,
    EnergyCapacity,
    BatteryDead,
    PowerOff,
    IsBoarded,
    IsOutOfWater,
    IsDocked,
    LookRightAxis,
    LookUpAxis,
    ForwardAxis,
    RightAxis,
    UpAxis,
    OverdriveActive,
    CameraDistance,
    PositionCameraBelowSub,
    Velocity,
    FreeCamera,
    TimeDelta,
    FixedTimeDelta,
    TargetScanTime,
    Health,
    MaxHealth,
    IsHealing,
    TriggerActive,
    OnboardingCooldown,
    OpenUpgradeCover,
    TorpedoMark,
    RailgunMark,
    IsFirstPerson,
}

