using Nautilus.Options.Attributes;
using Nautilus.Json;
using UnityEngine;

[Menu("Echelon Options")]
public class EchelonConfig : ConfigFile
{
    [Keybind("Input to Toggle Free Camera ")]
    public KeyCode toggleFreeCamera = KeyCode.F;
    [Toggle("Hold Sprint to Boost")]
    public bool holdToBoost = false;
    [Toggle("Ignore Torpedo Non-Target Collisions")]
    public bool ignoreTorpedoNonTargetCollisions = true;
    [Slider("Boost Acceleration %", Step = 1f, DefaultValue = 200, Min = 0, Max = 300)]
    public float boostAccelerationPercent = 200;
    [Slider("Nuclear Battery Energy Output %", Step = 1f, DefaultValue = 20, Min = 0, Max = 100)]
    public float batteryChargeSpeed = 20f;

}