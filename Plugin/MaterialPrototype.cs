using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using VehicleFramework;

namespace Subnautica_Echelon
{

    internal interface IShaderVariable
    {
        ShaderPropertyType Type { get; }
        void SetTo(Material m, bool verbose = false);
    }

    internal readonly struct ColorVariable : IShaderVariable
    {
        public ShaderPropertyType Type => ShaderPropertyType.Color;
        public Color Value { get; }
        public string Name { get; }
        public ColorVariable(Material m, string n)
        {
            Value = m.GetColor(n);
            Name = n;
        }

        public static void Set(Material m, string name, Color value, bool verbose)
        {
            try
            {
                var old = m.GetColor(name);
                if (old == value)
                    return;
                if (verbose)
                    Debug.Log($"Material correction: Applying Color {name} ({old} -> {value}) to material {m}");
                m.SetColor(name, value);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"Material correction: Failed to apply color {name} ({value}) to material {m}");
            }
        }

        public void SetTo(Material m, bool verbose)
        {
            Set(m, Name, Value, verbose);
        }
    }

    internal readonly struct VectorVariable : IShaderVariable
    {
        public ShaderPropertyType Type => ShaderPropertyType.Vector;


        public Vector4 Value { get; }
        public string Name { get; }
        public VectorVariable(Material m, string n)
        {
            Value = m.GetVector(n);
            Name = n;
        }

        public void SetTo(Material m, bool verbose)
        {
            try
            {
                var old = m.GetVector(Name);
                if (old == Value)
                    return;
                if (verbose)
                    Debug.Log($"Material correction: Applying {Type} {Name} ({old} -> {Value}) to material {m}");
                m.SetVector(Name, Value);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"Material correction: Failed to apply {Type} {Name} ({Value}) to material {m}");
            }
        }
    }
    
    internal readonly struct FloatVariable : IShaderVariable
    {
        public ShaderPropertyType Type => ShaderPropertyType.Float;


        public float Value { get; }
        public string Name { get; }
        public FloatVariable(Material m, string n)
        {
            Value = m.GetFloat(n);
            Name = n;
        }

        public void SetTo(Material m, bool verbose)
        {
            try
            {
                var old = m.GetFloat(Name);
                if (old == Value)
                    return;
                if (verbose)
                    Debug.Log($"Material correction: Applying {Type} {Name} ({old} -> {Value}) to material {m}");
                m.SetFloat(Name, Value);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"Material correction: Failed to apply {Type} {Name} ({Value}) to material {m}");
            }
        }
    }

    /// <summary>
    /// Read-only material definition as retrieved from some existing material
    /// </summary>
    /// <author>https://github.com/IronFox</author>
    public class MaterialPrototype
    {
        /// <summary>
        /// True if this instance was created without a source material
        /// </summary>
        public bool IsEmpty {get; private set; }
        
        private HashSet<string> ShaderKeywords { get; } = new HashSet<string>();
        public MaterialGlobalIlluminationFlags MaterialGlobalIlluminationFlags { get; }
        private List<ColorVariable> ColorVariables { get; }
            = new List<ColorVariable>();
        private List<VectorVariable> VectorVariables { get; }
            = new List<VectorVariable>();
        private List<FloatVariable> FloatVariables { get; }
            = new List<FloatVariable>();

        /// <summary>
        /// Updates all recorded shader variables in the specified material
        /// </summary>
        /// <param name="m">Target material</param>
        /// <param name="verbose">If true, every modification is logged</param>
        /// <param name="variableNamePredicate">
        /// Optional predicate to only check/update certain shader variables by name.
        /// If non-null updates only variables for which this function returns true</param>
        public void ApplyTo(Material m, bool verbose = false, Func<string,bool> variableNamePredicate = null)
        {
            variableNamePredicate = variableNamePredicate ?? (_ => true);

            foreach (var v in ColorVariables)
                if (variableNamePredicate(v.Name))
                    v.SetTo(m, verbose);
            foreach (var v in VectorVariables)
                if (variableNamePredicate(v.Name))
                    v.SetTo(m, verbose);
            foreach (var v in FloatVariables)
                if (variableNamePredicate(v.Name))
                    v.SetTo(m, verbose);

            if (m.globalIlluminationFlags != MaterialGlobalIlluminationFlags)
            {
                if (verbose)
                    Debug.Log($"Material correction: Applying global illumination flags ({m.globalIlluminationFlags} -> {MaterialGlobalIlluminationFlags})");

                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags;
            }

            foreach (var existing in m.shaderKeywords.ToList())
                if (!ShaderKeywords.Contains(existing))
                {
                    if (verbose)
                        Debug.Log($"Material correction: Removing shader keyword {existing}");
                    m.DisableKeyword(existing);
                }
            foreach (var kw in ShaderKeywords)
                if (!m.IsKeywordEnabled(kw))
                {
                    if (verbose)
                        Debug.Log($"Material correction: Enabling shader keyword {kw}");
                    m.EnableKeyword(kw);
                }
        }

        /// <summary>
        /// Constructs the prototype from a given material
        /// </summary>
        /// <param name="source"></param>
        public MaterialPrototype(Material source)
        {
            if (source == null)
            {
                IsEmpty = true;
                return;
            }
            MaterialGlobalIlluminationFlags = source.globalIlluminationFlags;
            ShaderKeywords.AddRange(source.shaderKeywords);

            for (int v = 0; v < source.shader.GetPropertyCount(); v++)
            {
                var n = source.shader.GetPropertyName(v);
                switch (source.shader.GetPropertyType(v))
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
                        if (!n.StartsWith("_Color"))    //don't  copy colors
                            ColorVariables.Add(new ColorVariable(source, n));
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
                        FloatVariables.Add(new FloatVariable(source, n));
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
                        VectorVariables.Add(new VectorVariable(source, n));
                        break;
                    //don't copy textures (does not make sense)
                    //case UnityEngine.Rendering.ShaderPropertyType.Texture:
                    //    if (n != "_MainTex" && n != "_BumpMap" && n != "_SpecTex" && n != "_Illum")
                    //        m.SetTexture(n, seamothMaterial.GetTexture(n));
                    //    break;
                }
            }
        }

        /// <summary>
        /// Creates a material prototype for the main material of the Seamoth body.
        /// While the Seamoth is not yet available, the method returns null.
        /// If the Seamoth is loaded but the material could not be found, the return
        /// value is an empty material prototype (IsEmpty=true)
        /// </summary>
        /// <param name="verbose">True to log details</param>
        /// <returns>Null if the seamoth is not (yet) available. Keep trying if null.
        /// Non-null if the seamoth is loaded, but can then be empty (IsEmpty is true)
        /// if the respective material is not found</returns>
        public static MaterialPrototype FromSeamoth(bool verbose=false)
        {
            var sm = SeamothHelper.Seamoth;
            if (sm == null)
                return null;

            if (verbose)
                Debug.Log($"Material correction: Found Seamoth");

            Material seamothMaterial = null;
            var renderers = sm.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                    if (material.shader.name == "MarmosetUBER"
                        && material.name.StartsWith("Submersible_SeaMoth"))
                    {
                        if (verbose)
                            Debug.Log($"Material correction: Found material to reproduce: {material.name}");
                        seamothMaterial = material;
                        break;
                    }
                    else
                    {
                        if (verbose)
                            Debug.Log($"Material correction: Shader mismatch {material.name} uses shader {material.shader.name}");
                    }
                if (seamothMaterial != null)
                    break;
            }
            return new MaterialPrototype(seamothMaterial);
        }
    }
}