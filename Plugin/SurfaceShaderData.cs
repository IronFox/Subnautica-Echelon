using System;
using UnityEngine;

namespace Subnautica_Echelon
{
    /// <summary>
    /// Surface shader data extracted from a material imported from Unity.
    /// Only values relevant to the translation process are read.
    /// Read-only
    /// </summary>
    /// <author>https://github.com/IronFox</author>
    public class SurfaceShaderData
    {
        /// <summary>
        /// Main color of the material. Black if none
        /// </summary>
        public Color Color { get; }
        /// <summary>
        /// Main texture of the material. Null if none.
        /// In order to be applicable as
        /// specular reflectivity map, its alpha value must be filled such.
        /// </summary>
        public Texture MainTex { get; }

        /// <summary>
        /// Smoothness value (typically 0-1)
        /// </summary>
        public float Smoothness { get; }
        /// <summary>
        /// Metallic texture. In order to be applicable as
        /// specular reflectivity map, its alpha value must be filled such.
        /// </summary>
        public Texture MetallicTexture { get; }
        /// <summary>
        /// Normal map. Null if none
        /// </summary>
        public Texture BumpMap { get; }
        /// <summary>
        /// Emission texture. Null if none
        /// </summary>
        public Texture EmissionTexture { get; }
        /// <summary>
        /// Texture channel to derive the smoothness (specular) appearance from
        /// 0 = Metallic
        /// 1 = MainTex
        /// </summary>
        public int SmoothnessTextureChannel { get; }

        public Texture SpecularTexture
        {
            get
            {
                switch (SmoothnessTextureChannel)
                {
                    case 0:
                        return MetallicTexture;
                    case 1:
                        return MainTex;
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// The source material
        /// </summary>
        public MaterialAddress Source { get; }

        public SurfaceShaderData(
            Color color,
            Texture mainTex,
            float smoothness,
            int smoothnessTextureChannel,
            Texture metallicTexture,
            Texture bumpMap,
            Texture emissionTexture,
            MaterialAddress source)
        {
            Source = source;
            Color = color;
            MainTex = mainTex;
            Smoothness = smoothness;
            MetallicTexture = metallicTexture;
            SmoothnessTextureChannel = smoothnessTextureChannel;
            BumpMap = bumpMap;
            EmissionTexture = emissionTexture;
        }

        private static Color GetColor(Material m, string name, LogConfig logConfig)
        {
            if (!m.HasProperty(name))
            {
                logConfig.LogWarning($"Material {m} does not have expected property {name}");
                return Color.black;
            }
            try
            {
                return m.GetColor(name);
            }
            catch (Exception e)
            {
                logConfig.LogError($"Material {m} does not have expected color property {name}");
                Debug.LogException(e);
                return Color.black;
            }
        }

        private static Texture GetTexture(Material m, string name, LogConfig logConfig)
        {
            if (!m.HasProperty(name))
            {
                logConfig.LogWarning($"Material {m} does not have expected property {name}");
                return null;
            }
            try
            {
                return m.GetTexture(name);
            }
            catch (Exception e)
            {
                logConfig.LogError($"Material {m} does not have expected texture property {name}");
                Debug.LogException(e);
                return null;
            }
        }

        private static float GetFloat(Material m, string name, LogConfig logConfig)
        {
            if (!m.HasProperty(name))
            {
                logConfig.LogWarning($"Material {m} does not have expected property {name}");
                return 0;
            }
            try
            {
                return m.GetFloat(name);
            }
            catch (Exception e)
            {
                logConfig.LogError($"Material {m} does not have expected float property {name}");
                Debug.LogException(e);
                return 0;
            }
        }

        private static int GetInt(Material m, string name, LogConfig logConfig)
        {
            if (!m.HasProperty(name))
            {
                logConfig.LogWarning($"Material {m} does not have expected property {name}");
                return 0;
            }
            try
            {
                return m.GetInt(name);
            }
            catch (Exception e)
            {
                logConfig.LogError($"Material {m} does not have expected int property {name}");
                Debug.LogException(e);
                return 0;
            }
        }

        [Obsolete("Please use SurfaceShaderData.From(renderer,materialIndex, logConfig) instead")]
        public static SurfaceShaderData From(Material m, bool ignoreShaderName = false)
        {
            return From(target: default, m, LogConfig.Default, ignoreShaderName);
        }

        private static SurfaceShaderData From(MaterialAddress target, Material m, LogConfig logConfig, bool ignoreShaderName = false)
        {

            if (m.shader.name != "Standard" && !ignoreShaderName)
            {
                logConfig.LogExtraStep($"Ignoring {m} which uses shader {m.shader}");
                return null;
            }
            logConfig.LogExtraStep($"Reading material {m} which uses shader {m.shader}");
            return new SurfaceShaderData(
                color: GetColor(m, "_Color", logConfig),
                mainTex: GetTexture(m, "_MainTex", logConfig),
                smoothness: GetFloat(m, "_Glossiness", logConfig),
                metallicTexture: GetTexture(m, "_MetallicGlossMap", logConfig),
                bumpMap: GetTexture(m, "_BumpMap", logConfig),
                emissionTexture: GetTexture(m, "_EmissionMap", logConfig),
                smoothnessTextureChannel: GetInt(m, "_SmoothnessTextureChannel", logConfig),
                source: target
                );
        }

        /// <summary>
        /// Reads all local values from the given material address (if available).
        /// Unless <paramref name="ignoreShaderName"/> is set,
        /// the method returns null if the material's shader's name does not
        /// currently match "Standard"
        /// </summary>
        /// <param name="source">The source material</param>
        /// <param name="ignoreShaderName">
        /// If true, will always read the material, regardless of shader name.
        /// If false, will only read the material if its shader name equals "Standard",
        /// return null otherwise</param>
        /// <returns>Read surface shader data or null if the shader name did not match
        /// or the target is (no longer) valid</returns>
        public static SurfaceShaderData From(MaterialAddress source, LogConfig logConfig, bool ignoreShaderName = false)
        {
            var material = source.GetMaterial();
            if (material == null)
            {
                Debug.LogError($"Material {source} could not be resolved to an instance");
                return null;
            }
            return From(source, material, logConfig, ignoreShaderName);
        }



        /// <summary>
        /// Reads all local values from the given renderer material (if available).
        /// Unless <paramref name="ignoreShaderName"/> is set,
        /// the method returns null if the material's shader's name does not
        /// currently match "Standard"
        /// </summary>
        /// <param name="renderer">The source renderer</param>
        /// <param name="materialIndex">The source material index on that renderer</param>
        /// <param name="ignoreShaderName">
        /// If true, will always read the material, regardless of shader name.
        /// If false, will only read the material if its shader name equals "Standard",
        /// return null otherwise</param>
        /// <returns>Read surface shader data or null if the shader name did not match
        /// or the target is (no longer) valid</returns>
        public static SurfaceShaderData From(Renderer renderer, int materialIndex, LogConfig logConfig = default, bool ignoreShaderName = false)
        {
            return From(new MaterialAddress(renderer, materialIndex), logConfig);
        }


        private const string SpecTexName = "_SpecTex";
        private const string IllumTexName = "_Illum";
        private const string DummyTexName = "SurfaceShaderData.DummyTexture";
        /// <summary>
        /// Applies the loaded configuration to the given material
        /// </summary>
        /// <param name="m">Target material</param>
        /// <param name="logConfig">Log Configuration</param>
        public void ApplyTo(Material m, float? uniformShininess, LogConfig logConfig)
        {
            ColorVariable.Set(m, "_Color2", Color, logConfig);
            ColorVariable.Set(m, "_Color3", Color, logConfig);

            var existingSpecTex = m.GetTexture(SpecTexName);

            var spec = SpecularTexture;

            if (spec && uniformShininess is null)
            {
                if (existingSpecTex != MetallicTexture)
                {
                    logConfig.LogExtraStep($"Translating smoothness alpha map {spec} to spec");

                    m.SetTexture(SpecTexName, MetallicTexture);
                }
            }
            else
            {
                var tex = existingSpecTex as Texture2D;
                if (!tex || existingSpecTex.name != DummyTexName)
                {
                    logConfig.LogExtraStep($"Source has no smoothness alpha texture. Setting to {Smoothness}");
                    var gray = uniformShininess ?? Smoothness;
                    tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    tex.name = DummyTexName;
                    tex.SetPixel(0, 0, new Color(gray, gray, gray, gray));
                    tex.Apply();
                    m.SetTexture(SpecTexName, tex);
                }
                else
                {
                    var gray = uniformShininess ?? Smoothness;
                    var col = new Color(gray, gray, gray, gray);
                    if (tex.GetPixel(0, 0) != col)
                    {
                        logConfig.LogExtraStep($"Updating smoothness alpha texture. Setting to {gray}");
                        tex.SetPixel(0, 0, col);
                        tex.Apply();
                    }
                }
            }
            var existingIllumTex = m.GetTexture(IllumTexName);

            if (EmissionTexture != null)
            {
                if (EmissionTexture != existingIllumTex)
                {
                    logConfig.LogExtraStep($"Translating emission map {EmissionTexture} to _Illum");

                    m.SetTexture(IllumTexName, EmissionTexture);
                }

            }
            else
            {
                if (existingIllumTex != Texture2D.blackTexture)
                {
                    logConfig.LogExtraStep($"Source has no illumination texture. Loading black into _Illum");
                    m.SetTexture(IllumTexName, Texture2D.blackTexture);
                }
            }
        }

        /// <summary>
        /// Creates a clone with a new source material address
        /// </summary>
        /// <param name="source">New source address</param>
        /// <returns>Clone with updated source</returns>
        public SurfaceShaderData RedefineSource(MaterialAddress source)
            => new SurfaceShaderData(
                color: Color,
                mainTex: MainTex,
                smoothness: Smoothness,
                metallicTexture: MetallicTexture,
                bumpMap: BumpMap,
                emissionTexture: EmissionTexture,
                smoothnessTextureChannel: SmoothnessTextureChannel,
                source: source);

        public override string ToString()
            => "" + Source;

    }
}