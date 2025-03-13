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

        public static SurfaceShaderData From(Material m, bool ignoreShaderName=false)
        {
            if (m.shader.name != "Standard" && !ignoreShaderName)
            {
                Debug.Log($"Material correction: Ignoring {m} which uses shader {m.shader}");
                return null;
            }
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

        private const string SpecTexName = "_SpecTex";
        private const string IllumTexName = "_Illum";
        private const string DummyTexName = "SurfaceShaderData.DummyTexture";
        public void ApplyTo(Material m, bool verbose)
        {
            ColorVariable.Set(m, "_Color2", Color, verbose);
            ColorVariable.Set(m, "_Color3", Color, verbose);
            
            var existingSpecTex = m.GetTexture(SpecTexName);

            if (MetallicTexture != null)
            {
                if (existingSpecTex != MetallicTexture)
                {
                    if (verbose)
                        Debug.Log($"Material correction: Translating metallic map {MetallicTexture} to spec");

                    m.SetTexture(SpecTexName, MetallicTexture);
                }
            }
            else
            {
                if (existingSpecTex == null || existingSpecTex.name != DummyTexName)
                {
                    if (verbose)
                        Debug.Log($"Material correction: Source had no metallic texture. Setting to {Metallic}");
                    var met = Metallic;
                    var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    tex.name = DummyTexName;
                    tex.SetPixel(0, 0, new Color(met, met, met, met));
                    tex.Apply();
                    m.SetTexture(SpecTexName, tex);
                }
            }

            var existingIllumTex = m.GetTexture(IllumTexName);

            if (EmissionTexture != null)
            {
                if (EmissionTexture != existingIllumTex)
                {
                    if (verbose)
                        Debug.Log($"Material correction: Translating emission map {EmissionTexture} to _Illum");

                    m.SetTexture(IllumTexName, EmissionTexture);
                }

            }
            else
            {
                if (existingIllumTex != Texture2D.blackTexture)
                {
                    if (verbose)
                        Debug.Log($"Material correction: Source had no illumination texture. Loading black into _Illum");
                    m.SetTexture("_Illum", Texture2D.blackTexture);
                }
            }
        }
    }
}