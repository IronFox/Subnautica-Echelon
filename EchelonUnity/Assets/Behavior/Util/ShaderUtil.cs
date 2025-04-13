using UnityEngine;
using UnityEngine.Rendering;

public static class ShaderUtil
{
    public static MaterialAccess Access(Transform t, int materialIndex = 0)
    {
        var renderer = t.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            ULog.Warn($"MeshRenderer component expected in {t.name}[{t.GetInstanceID()}]");
            return default;
        }

        if (renderer.materials.Length <= materialIndex)
        {
            ULog.Warn($"MeshRenderer ({t.name}[{t.GetInstanceID()}]) expected to have at least {materialIndex} material(s) but has {renderer.materials.Length}");
            return default;
        }

        return new MaterialAccess(t, renderer.materials[materialIndex]);

    }

}

public readonly struct MaterialAccess
{
    public MaterialAccess(Transform t, Material material)
    {
        Transform = t;
        Material = material;
    }

    public static MaterialAccess From(Transform t)
        => ShaderUtil.Access(t);

    public Transform Transform { get; }
    public Material Material { get; }

    public bool IsActive => Transform != null && Material != null;

    private string Target =>
        $"MeshRenderer ('{Transform.name}'[{Transform.GetInstanceID()}]) material '{Material.name}' shader '{Material.shader.name}'";

    private bool AssertHasProperty(string name, ShaderPropertyType type)
    {
        if (Transform == null || Material == null)
        {
            return false;
        }
        int idx = -1;
        for (int i = 0; i < Material.shader.GetPropertyCount(); i++)
            if (Material.shader.GetPropertyName(i) == name)
            {
                idx = i;
                break;
            }
        if (idx < 0)
        {
            ULog.Warn($"{Target} does not have property '{name}'");
            return false;
        }
        var t = Material.shader.GetPropertyType(idx);
        if (t != type)
        {
            ULog.Warn($"{Target} property type {t} does not match expected type {type}");
            return false;
        }
        return true;
    }

    public bool SetFloat(string name, float value)
    {
        if (!AssertHasProperty(name, ShaderPropertyType.Float))
            return false;
        var current = Material.GetFloat(name);
        if (current != value)
        {
            ULog.Write($"Setting {Target} float {name} {current} -> {value}");
            Material.SetFloat(name, value);
        }
        return true;
    }

    public bool SetTexture(string name, Texture value)
    {
        if (!AssertHasProperty(name, ShaderPropertyType.Texture))
            return false;
        var current = Material.GetTexture(name);
        if (current != value)
        {
            ULog.Write($"Setting {Target} texture {name} {current} -> {value}");
            Material.SetTexture(name, value);
        }
        return true;
    }
}