using System;
using UnityEngine;

public static class MaterialAdapter
{

    public static Action<EchelonControl, Renderer, int, Texture> UpdateMainTexture { get; set; } = (echelon, renderer, materialIndex, tex) => { renderer.materials[materialIndex].mainTexture = tex; };

    public static Action<EchelonControl, Renderer, int, Color, float> UpdateColorSmoothness { get; set; } = (echelon, renderer, materialIndex, color, smoothness) =>
    {
        var mat = renderer.materials[materialIndex];
        mat.color = color;
        mat.SetFloat("_Glossiness", smoothness);
    };
}
