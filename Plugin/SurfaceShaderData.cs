using System;
using UnityEngine;

namespace Subnautica_Echelon
{
    public class SurfaceShaderData
    {
        public Color Color { get; }
        public Texture MainTex { get; }
        public float Metallic { get; }
        public Texture MetallicTexture { get; }
        public Texture BumpMap { get; }
        public Texture EmissionTexture { get; }

        public SurfaceShaderData(
            Color color,
            Texture mainTex,
            float metallic,
            Texture metallicTexture,
            Texture bumpMap,
            Texture emissionTexture)
        {
            Color = color;
            MainTex = mainTex;
            Metallic = metallic;
            MetallicTexture = metallicTexture;
            BumpMap = bumpMap;
            EmissionTexture = emissionTexture;
        }

        private static Color GetColor(Material m, string name)
        {
            if (!m.HasProperty(name))
            {
                Debug.LogWarning($"Material {m.name} does not have expected property {name}");
                return Color.black;
            }
            try
            {
                return m.GetColor(name);
            }
            catch (Exception e)
            {
                Debug.LogError($"Material {m.name} does not have expected color property {name}");
                Debug.LogException(e);
                return Color.black;
            }
        }
        
        private static Texture GetTexture(Material m, string name)
        {
            if (!m.HasProperty(name))
            {
                Debug.LogWarning($"Material {m.name} does not have expected property {name}");
                return null;
            }
            try
            {
                return m.GetTexture(name);
            }
            catch (Exception e)
            {
                Debug.LogError($"Material {m.name} does not have expected texture property {name}");
                Debug.LogException(e);
                return null;
            }
        }
        
        private static float GetFloat(Material m, string name)
        {
            if (!m.HasProperty(name))
            {
                Debug.LogWarning($"Material {m.name} does not have expected property {name}");
                return 0;
            }
            try
            {
                return m.GetFloat(name);
            }
            catch (Exception e)
            {
                Debug.LogError($"Material {m.name} does not have expected float property {name}");
                Debug.LogException(e);
                return 0;
            }
        }

        public static SurfaceShaderData From(Material m)
        {
            return new SurfaceShaderData(
                color: GetColor(m,"_Color"),
                mainTex: GetTexture(m,"_MainTex"),
                metallic: GetFloat(m, "_Metallic"),
                metallicTexture: GetTexture(m, "_MetallicGlossMap"),
                bumpMap: GetTexture(m, "_BumpMap"),
                emissionTexture: GetTexture(m, "_EmissionMap")
                );
        }
    }
}