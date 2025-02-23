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
}