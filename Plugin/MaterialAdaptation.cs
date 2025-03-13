using System;
using UnityEngine;
using static PDAScanner;
using static UnityEngine.GraphicsBuffer;

namespace Subnautica_Echelon
{
    /// <summary>
    /// A renderer material target description, identifying a material by its slot index,
    /// not reference.
    /// </summary>
    public readonly struct MaterialAdaptationTarget : IEquatable<MaterialAdaptationTarget>
    {
        /// <summary>
        /// The targeted renderer. Can become null if the source is destroyed
        /// </summary>
        public MeshRenderer Renderer { get; }
        /// <summary>
        /// The recorded instance id of the renderer. Preserved for performance and also
        /// to prevent null reference exceptions if the renderer is destroyed
        /// </summary>
        public int RendererInstanceId { get; }
        /// <summary>
        /// The 0-based index of this material on the targeted renderer
        /// </summary>
        public int MaterialIndex { get; }

        public override string ToString()
        {
            if (Renderer == null)
                return $"Dead Renderer Target ({RendererInstanceId}) material #{MaterialIndex}";
            return $"Renderer Target {Renderer} material #{MaterialIndex}";
        }
        public MaterialAdaptationTarget(MeshRenderer renderer, int materialIndex)
        {
            RendererInstanceId = renderer.GetInstanceID();
            Renderer = renderer;
            MaterialIndex = materialIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is MaterialAdaptationTarget target &&
                    Equals(target);
        }

        public override int GetHashCode()
        {
            int hashCode = 570612675;
            hashCode = hashCode * -1521134295 + RendererInstanceId.GetHashCode();
            hashCode = hashCode * -1521134295 + MaterialIndex.GetHashCode();
            return hashCode;
        }

        public bool Equals(MaterialAdaptationTarget target)
        {
            return RendererInstanceId == target.RendererInstanceId &&
                   MaterialIndex == target.MaterialIndex;
        }
    }

    /// <summary>
    /// A full material translation migrated+prototype -> final
    /// </summary>
    public class MaterialAdaptation
    {
        /// <summary>
        /// The targeted material
        /// </summary>
        public MaterialAdaptationTarget Target { get; }
        /// <summary>
        /// The (shared) prototype used to modify the final material
        /// </summary>
        public MaterialPrototype Prototype { get; }
        /// <summary>
        /// The data migrated from the original material as present in the mesh
        /// </summary>
        public SurfaceShaderData Migrated { get; }
        /// <summary>
        /// The shader that is to be applied to the material
        /// </summary>
        public Shader Shader { get; }

        public MaterialAdaptation(
            MeshRenderer renderer,
            int materialIndex,
            MaterialPrototype prototype,
            SurfaceShaderData surfaceShaderData,
            Shader shader
            ) : this(new MaterialAdaptationTarget(renderer,materialIndex), prototype, surfaceShaderData, shader)
        {}

        public MaterialAdaptation(
            MaterialAdaptationTarget materialAdaptationTarget,
            MaterialPrototype prototype,
            SurfaceShaderData migrated,
            Shader shader
            )
        {
            Target = materialAdaptationTarget;
            Prototype = prototype;
            Migrated = migrated;
            Shader = shader;
        }

        /// <summary>
        /// Resets only variables known to be corrupted during moonpool undock
        /// </summary>
        /// <param name="verbose">If true, every modification is logged</param>
        public void PostDockFixOnTarget(bool verbose = true)
        {
            try
            {
                var r = Target.Renderer;
                if (r == null)
                {
                    Debug.LogWarning($"Material correction: Target renderer is gone ({Target.RendererInstanceId}). Cannot apply");
                    return;
                }
                if (Target.MaterialIndex >= r.materials.Length)
                {
                    Debug.LogWarning($"Material correction: Target renderer has only {r.materials.Length} materials, but needs {Target.MaterialIndex + 1}. Cannot apply");
                    return;
                }
                var m = r.materials[Target.MaterialIndex];
                if (m.shader != Shader)
                {
                    if (verbose)
                        Debug.Log($"Material correction: Applying shader {Shader.name} to target");

                    m.shader = Shader;
                }

                Prototype.ApplyTo(m, verbose, x =>
                       x == "_SpecInt"
                    || x == "_GlowStrength"
                    || x == "_GlowStrengthNight");

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"Material correction: Failed to apply MaterialAdaptation to material {Target}");
            }
        }

        /// <summary>
        /// Reapplies all material properties to the target
        /// </summary>
        /// <param name="verbose">If true, every modification is logged</param>
        public void ApplyToTarget(bool verbose = false)
        {
            try
            {
                var r = Target.Renderer;
                if (r == null)
                {
                    Debug.LogWarning($"Material correction: Target renderer is gone ({Target.RendererInstanceId}). Cannot apply");
                    return;
                }
                if (Target.MaterialIndex >= r.materials.Length)
                {
                    Debug.LogWarning($"Material correction: Target renderer has only {r.materials.Length} materials, but needs {Target.MaterialIndex+1}. Cannot apply");
                    return;
                }
                var m = r.materials[Target.MaterialIndex];
                if (m.shader != Shader)
                {
                    if (verbose)
                        Debug.Log($"Material correction: Applying shader {Shader.name} to target");

                    m.shader = Shader;
                }

                Prototype.ApplyTo(m, verbose);

                Migrated.ApplyTo(m, verbose);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"Material correction: Failed to apply MaterialAdaptation to material {Target}");
            }
        }
    }
}