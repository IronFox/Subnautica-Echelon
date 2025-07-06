using Nautilus.Json;
using Nautilus.Options.Attributes;
using UnityEngine;

[Menu("Echelon Options")]
public class EchelonConfig : ConfigFile
{
    [Keybind("Toggle Free Camera", LabelLanguageId = "optInput_ToggleFreeCamera")]
    public KeyCode toggleFreeCamera = KeyCode.F;
    [Keybind("Camera Zoom In", LabelLanguageId = "optInput_CameraZoomIn")]
    public KeyCode altZoomIn = KeyCode.None;
    [Keybind("Camera Zoom Out", LabelLanguageId = "optInput_CameraZoomOut")]
    public KeyCode altZoomOut = KeyCode.None;
    [Toggle("Hold Sprint to Boost", LabelLanguageId = "optInput_HoldToBoost")]
    public bool holdToBoost = false;
    [Choice("Torpedo Terrain Collisions",
        nameof(TorpedoTerrainCollisions.NeverIgnore),
        nameof(TorpedoTerrainCollisions.IgnoreWhenTargeted),
        nameof(TorpedoTerrainCollisions.AlwaysIgnore),
        LabelLanguageId = "optTorpedoTerrainCollisions"
        )]
    public TorpedoTerrainCollisions torpedoTerrainCollisions = TorpedoTerrainCollisions.IgnoreWhenTargeted;
    [Choice("Target Text",
        nameof(TargetDisplay.None),
        nameof(TargetDisplay.Focused),
        nameof(TargetDisplay.All),
        nameof(TargetDisplay.LockedOnly),
        LabelLanguageId = "optTargetText")]
    public TargetDisplay textDisplay = TargetDisplay.All;
    [Choice("Target Health Marker Display",
        nameof(TargetDisplay.None),
        nameof(TargetDisplay.Focused),
        nameof(TargetDisplay.All),
        nameof(TargetDisplay.LockedOnly),
        LabelLanguageId = "optTargetHealthMarker")]
    public TargetDisplay targetHealthMarkers = TargetDisplay.All;
    [Choice("Target Arrows",
        nameof(TargetArrows.DangerousAndCriticialTargets),
        nameof(TargetArrows.CriticalOnly),
        nameof(TargetArrows.None),
        LabelLanguageId = "optTargetArrows"
        )]
    public TargetArrows targetArrows = TargetArrows.DangerousAndCriticialTargets;
    [Slider(DefaultValue = 100, Format = "{0:F0} %", Label = "Additional look sensitivity", LabelLanguageId = "optLookSensitivity", Min = 10, Max = 200, Step = 10)]
    public float lookSensitivity = 100;
    [Slider(DefaultValue = 100, Format = "{0:F0} %", Label = "Target Marker Size Scale", LabelLanguageId = "optTargetMarkerSizeScale", Min = 0, Max = 100, Step = 10)]
    public float targetMarkerSizeScale = 100;
    [Slider(DefaultValue = 1700, Label = "Mk1 Railgun Damage", LabelLanguageId = "optMk1RailgunDamage", Min = 0, Max = 10000, Step = 100)]
    public float mk1RailgunDamage = 1700;
    [Slider(DefaultValue = 4000, Label = "Mk2 Railgun Damage", LabelLanguageId = "optMk2RailgunDamage", Min = 0, Max = 10000, Step = 100)]
    public float mk2RailgunDamage = 4000;
    [Slider(DefaultValue = 75, Format = "{0:F0} %", Label = "Railgun Sound Level", LabelLanguageId = "optRailgunSoundLevel", Min = 0, Max = 100, Step = 5)]
    public float railgunSoundLevel = 75;

    [Toggle("Uniform Shininess", LabelLanguageId = "optUniformShininess")]
    public bool uniformShininess = false;
    [Slider(DefaultValue = 50, Format = "{0:F0} %", Label = "Uniform Shininess Level", LabelLanguageId = "optUniformShininessLevel", Min = 0, Max = 100, Step = 5)]
    public float uniformShininessLevel = 50;
    [Slider(DefaultValue = 100, Format = "{0:F0} %", Label = "Global Camera Shake Intensity", LabelLanguageId = "optGlobalCameraShakeIntensity", Min = 0, Max = 200, Step = 10)]
    public float globalCameraShakeIntensity = 100;
    [Slider(DefaultValue = 100, Format = "{0:F0} %", Label = "Turbo Camera Shake Intensity", LabelLanguageId = "optBoostCameraShakeIntensity", Min = 0, Max = 200, Step = 10)]
    public float boostCameraShakeIntensity = 100;
    [Slider(DefaultValue = 50, Format = "{0:F0} %", Label = "Engine Sound Volume", LabelLanguageId = "optEngineSoundVolume", Min = 0, Max = 200, Step = 10)]
    public float engineSoundVolume = 50;

}