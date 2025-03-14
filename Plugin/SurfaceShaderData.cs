using System;
using UnityEngine;
using static PDAScanner;

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
        /// Main color of the material. Black if known
        /// </summary>
        public Color Color { get; }
        /// <summary>
        /// Main texture of the material. Null if none
        /// </summary>
        public Texture MainTex { get; }
        /// <summary>
        /// Metallic value (typically 0-1)
        /// </summary>
        public float Metallic { get; }
        /// <summary>
        /// Metallic texture. In order to be applicable as
        /// specular reflectivity map, its alpha value must be filled such.
        /// As far as known, the RGB channels are not used for this purpose.
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
        /// The source material
        /// </summary>
        public MaterialTarget Source { get; }

        public SurfaceShaderData(
            Color color,
            Texture mainTex,
            float metallic,
            Texture metallicTexture,
            Texture bumpMap,
            Texture emissionTexture,
            MaterialTarget source)
        {
            Source = source;
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

        [Obsolete("Please use SurfaceShaderData.From(renderer,materialIndex) instead")]
        public static SurfaceShaderData From(Material m, bool ignoreShaderName = false)
        {
            return From(target:default, m, ignoreShaderName);
        }

        private static SurfaceShaderData From(MaterialTarget target, Material m, bool ignoreShaderName = false)
        {

            if (m.shader.name != "Standard" && !ignoreShaderName)
            {
                Debug.Log($"Material correction: Ignoring {m} which uses shader {m.shader}");
                return null;
            }
            Debug.Log($"Material correction: Reading material {m.name} which uses shader {m.shader.name}");
            return new SurfaceShaderData(
                color: GetColor(m, "_Color"),
                mainTex: GetTexture(m, "_MainTex"),
                metallic: GetFloat(m, "_Metallic"),
                metallicTexture: GetTexture(m, "_MetallicGlossMap"),
                bumpMap: GetTexture(m, "_BumpMap"),
                emissionTexture: GetTexture(m, "_EmissionMap"),
                source: target
                );
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
        /// <returns>Read surface shader data or null if the shader name did not match</returns>
        public static SurfaceShaderData From(Renderer renderer, int materialIndex, bool ignoreShaderName=false)
        {
            if (renderer == null)
            {
                Debug.LogError($"Trying to feed null renderer into SurfaceShaderData.From()");
                return null;
            }

            if (materialIndex >= renderer.materials.Length)
            {
                Debug.LogError($"Renderer {renderer} does not have material #{materialIndex}");
                return null;
            }

            var m = renderer.materials[materialIndex];
            return From(new MaterialTarget(renderer, materialIndex), m, ignoreShaderName);
        }


        private const string SpecTexName = "_SpecTex";
        private const string IllumTexName = "_Illum";
        private const string DummyTexName = "SurfaceShaderData.DummyTexture";
        /// <summary>
        /// Applies the loaded configuration to the given material
        /// </summary>
        /// <param name="m">Target material</param>
        /// <param name="verbose">If true, every modification is logged</param>
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

        internal SurfaceShaderData RedefineSource(MaterialTarget source)
            => new SurfaceShaderData(Color, MainTex, Metallic, MetallicTexture, BumpMap, EmissionTexture, source);
        
    }
}