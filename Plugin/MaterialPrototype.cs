using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using VehicleFramework;

namespace Subnautica_Echelon
{

    public interface IShaderVariable
    {
        ShaderPropertyType Type { get; }
        void SetTo(Material m, bool verbose = false);
    }

    public readonly struct ColorVariable : IShaderVariable
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
    
    public readonly struct VectorVariable : IShaderVariable
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
    
    public readonly struct FloatVariable : IShaderVariable
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
    /// Readonly material definition as retrieved from some existing material
    /// </summary>
    public class MaterialPrototype
    {
        public bool IsEmpty {get; private set; }
        
        private HashSet<string> ShaderKeywords { get; } = new HashSet<string>();
        public MaterialGlobalIlluminationFlags MaterialGlobalIlluminationFlags { get; }
        private List<ColorVariable> ColorVariables { get; }
            = new List<ColorVariable>();
        private List<VectorVariable> VectorVariables { get; }
            = new List<VectorVariable>();
        private List<FloatVariable> FloatVariables { get; }
            = new List<FloatVariable>();

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

            if (ShaderKeywordsDifferent(m.shaderKeywords))
            {
                if (verbose)
                    Debug.Log($"Material correction: Applying shader keywords");
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
            //if (verbose)
            //    Debug.Log($"Material correction: Prototype applied");

        }

        private bool ShaderKeywordsDifferent(string[] shaderKeywords)
        {
            foreach (string s in shaderKeywords)
                if (!ShaderKeywords.Contains(s))
                    return true;
            return shaderKeywords.Length != ShaderKeywords.Count;
        }

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
                        if (!n.StartsWith("_Color"))
                            ColorVariables.Add(new ColorVariable(source, n));
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
                        FloatVariables.Add(new FloatVariable(source, n));
                        break;
                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
                        VectorVariables.Add(new VectorVariable(source, n));
                        break;
                    //case UnityEngine.Rendering.ShaderPropertyType.Texture:
                    //    if (n != "_MainTex" && n != "_BumpMap" && n != "_SpecTex" && n != "_Illum")
                    //        m.SetTexture(n, seamothMaterial.GetTexture(n));
                    //    break;
                }
            }
        }

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