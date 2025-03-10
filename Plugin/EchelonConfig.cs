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
    [Choice("Target Text", nameof(TextDisplay.None), nameof(TextDisplay.Focused), nameof(TextDisplay.All))]
    public TextDisplay textDisplay = TextDisplay.All;
    [Choice("Target Arrows", nameof(TargetArrows.DangerousAndCriticialTargets), nameof(TargetArrows.CriticalOnly), nameof(TargetArrows.None))]
    public TargetArrows targetArrows = TargetArrows.DangerousAndCriticialTargets;

}