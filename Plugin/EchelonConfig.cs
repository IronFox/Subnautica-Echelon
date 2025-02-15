using Nautilus.Options.Attributes;
using Nautilus.Json;
using UnityEngine;

[Menu("Echelon Options")]
public class EchelonConfig : ConfigFile
{
    [Keybind("Toggle Free Camera")]
    public KeyCode toggleFreeCamera = KeyCode.F;

}