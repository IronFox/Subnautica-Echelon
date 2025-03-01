using Nautilus.Options.Attributes;
using Nautilus.Json;
using UnityEngine;
using System;

[Menu("Echelon Options")]
public class EchelonConfig : ConfigFile
{
    [Keybind("Input to Toggle Free Camera ")]
    public KeyCode toggleFreeCamera = KeyCode.F;
    [Keybind("Input to Reduce the 3rd Person Camera")]
    public KeyCode altZoomIn = KeyCode.None;
    [Keybind("Input to Increase the 3rd Person Camera")]
    public KeyCode altZoomOut = KeyCode.None;
    [Toggle("Hold Sprint to Boost")]
    public bool holdToBoost = false;
    [Choice("Torpedo Terrain Collisions",
        nameof(TorpedoTerrainCollisions.NeverIgnore),
        nameof(TorpedoTerrainCollisions.IgnoreWhenTargeted),
        nameof(TorpedoTerrainCollisions.AlwaysIgnore)
        )]
    public TorpedoTerrainCollisions torpedoTerrainCollisions = TorpedoTerrainCollisions.IgnoreWhenTargeted;
    [Slider("Boost Acceleration %", Step = 1f, DefaultValue = 150, Min = 0, Max = 300)]
    public float boostAccelerationPercent = 150;
    [Slider("Nuclear Battery Energy Output %", Step = 1f, DefaultValue = 20, Min = 0, Max = 100)]
    public float batteryChargeSpeed = 20f;
    [Slider("Self Healing %", Step = 1f, DefaultValue = 10, Min = 0, Max = 100)]
    public float selfHealingSpeed = 10f;
    [Slider("Torpedo Damage", Step = 10, DefaultValue = 1500, Min = 10, Max = 3000)]
    public float torpedoDamage = 1500f;
    [Slider("Torpedoes per Minute", Step = 1, DefaultValue = 60, Min = 5, Max = 360)]
    public float torpedoesPerMinute = 60;
    [Choice("Target Text", nameof(TextDisplay.None), nameof(TextDisplay.Focused), nameof(TextDisplay.All))]
    public TextDisplay textDisplay = TextDisplay.All;
    [Choice("Target Arrows", nameof(TargetArrows.DangerousAndCriticialTargets), nameof(TargetArrows.CriticalOnly), nameof(TargetArrows.None))]
    public TargetArrows targetArrows = TargetArrows.DangerousAndCriticialTargets;

}