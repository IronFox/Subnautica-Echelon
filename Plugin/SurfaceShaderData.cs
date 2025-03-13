using System;
using UnityEngine;
using static PDAScanner;

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
                Debug.LogWarning($"Material correction: Material {m.name} does not have expected property {name}");
                return Color.black;
            }
            try
            {
                return m.GetColor(name);
            }
            catch (Exception e)
            {
                Debug.LogError($"Material correction: Material {m.name} does not have expected color property {name}");
                Debug.LogException(e);
                return Color.black;
            }
        }
        
        private static Texture GetTexture(Material m, string name)
        {
            if (!m.HasProperty(name))
            {
                Debug.LogWarning($"Material correction: Material {m.name} does not have expected property {name}");
                return null;
            }
            try
            {
                return m.GetTexture(name);
            }
            catch (Exception e)
            {
                Debug.LogError($"Material correction: Material {m.name} does not have expected texture property {name}");
                Debug.LogException(e);
                return null;
            }
        }
        
        private static float GetFloat(Material m, string name)
        {
            if (!m.HasProperty(name))
            {
                Debug.LogWarning($"Material correction: Material {m.name} does not have expected property {name}");
                return 0;
            }
            try
            {
                return m.GetFloat(name);
            }
            catch (Exception e)
            {
                Debug.LogError($"Material correction: Material {m.name} does not have expected float property {name}");
                Debug.LogException(e);
                return 0;
            }
        }

        public static SurfaceShaderData From(Material m)
        {
            Debug.Log($"Material correction: Reading material {m.name} which uses shader {m.shader.name}");
            return new SurfaceShaderData(
                color: GetColor(m,"_Color"),
                mainTex: GetTexture(m,"_MainTex"),
                metallic: GetFloat(m, "_Metallic"),
                metallicTexture: GetTexture(m, "_MetallicGlossMap"),
                bumpMap: GetTexture(m, "_BumpMap"),
                emissionTexture: GetTexture(m, "_EmissionMap")
                );
        }

        public void ApplyTo(Material m, bool verbose)
        {

            m.SetColor("_Color2", Color);
            m.SetColor("_Color3", Color);
            if (MetallicTexture != null)
            {
                if (verbose)
                    Debug.Log($"Material correction: Translating metallic map {MetallicTexture} to spec");

                m.SetTexture("_SpecTex", MetallicTexture);
            }
            else
            {
                if (verbose)
                    Debug.Log($"Material correction: Source had no metallic texture. Setting to {Metallic}");

                var existing = m.GetTexture("_SpecTex");
                if (existing != null && existing.name == $"SurfaceShaderData.DummyTexture")
                    GameObject.Destroy(existing);

                var met = Metallic;
                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.name = $"SurfaceShaderData.DummyTexture";
                tex.SetPixel(0, 0, new Color(met, met, met, met));
                tex.Apply();
                m.SetTexture("_SpecTex", tex);
            }

            if (EmissionTexture != null)
            {
                if (verbose)
                    Debug.Log($"Material correction: Translating emission map {EmissionTexture} to _Illum");

                m.SetTexture("_Illum", EmissionTexture);

            }
            else
            {
                if (verbose)
                    Debug.Log($"Material correction: Source had no illumination texture. Loading black into _Illum");
                m.SetTexture("_Illum", Texture2D.blackTexture);
            }
        }
    }
}