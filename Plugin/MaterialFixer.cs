using System;
using System.Collections.Generic;
using UnityEngine;

namespace Subnautica_Echelon
{
    /// <summary>
    /// Helper class to fix materials automatically. Should be instantiated on the vehicle
    /// you wish to fix materials of
    /// </summary>
    /// <author>https://github.com/IronFox</author>
    public class MaterialFixer
    {

        private DateTime repairMaterialsIn = DateTime.MaxValue;
        private int repairMaterialsInFrames = 3;
        private bool materialsFixed;
        private readonly List<MaterialAdaptation> adaptations = new List<MaterialAdaptation>();

        public bool MaterialsAreFixed => materialsFixed;

        public bool VerboseLogging { get; set; }

        public Func<Transform, IEnumerable<SurfaceShaderData>> MaterialResolver { get; }

        /// <summary>
        /// Constructs the instance
        /// </summary>
        /// <param name="materialResolver">The solver function to fetch all materials to translate
        /// of a root transform. If null, a default implementation is used</param>
        /// <param name="verbose">Log verbosely. Can be changed any time</param>
        public MaterialFixer(
            Func<Transform, IEnumerable<SurfaceShaderData>> materialResolver = null,
            bool verbose=false
            )
        { 
            VerboseLogging = verbose;
            MaterialResolver = materialResolver ?? DefaultMaterialResolver;
        }

        private static IEnumerable<SurfaceShaderData> DefaultMaterialResolver(Transform transform)
        {
            var renderers = transform.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.gameObject.name.ToLower().Contains("light"))
                    continue;
                // copied from VF default behavior:
                if (renderer.gameObject.GetComponent<Skybox>())
                {
                    // I feel okay using Skybox as the designated "don't apply marmoset to me" component.
                    // I think there's no reason a vehicle should have a skybox anywhere.
                    // And if there is, I'm sure that developer can work around this.
                    Component.DestroyImmediate(renderer.gameObject.GetComponent<Skybox>());
                    continue;
                }

                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    var material = SurfaceShaderData.From(renderer, i);
                    if (material != null)
                        yield return material;
                }
            }
        }

        /// <summary>
        /// Notifies that the vehicle has just undocked from a docking bay (moonpool, etc)
        /// </summary>
        public void OnVehicleUndocked()
        {
            repairMaterialsIn = DateTime.Now + TimeSpan.FromSeconds(0.5f);
            repairMaterialsInFrames = 3;
        }

        /// <summary>
        /// Forcefully reapplies all material adaptations.
        /// Normally not necessary
        /// </summary>
        public void ReApply()
        {
            foreach (MaterialAdaptation adaptation in adaptations)
                adaptation.ApplyToTarget(VerboseLogging);
        }

        /// <summary>
        /// Fixed materials if necessary/possible.
        /// Also fixes undock material changes if <see cref="OnVehicleUndocked"/> was called before
        /// </summary>
        /// <remarks>Should be called once during your vehicle Update()</remarks>
        /// <param name="subTransform">Root transform of your sub</param>
        public void OnUpdate(Transform subTransform)
        {

            if (!materialsFixed)
            {
                var prototype = MaterialPrototype.FromSeamoth();

                if (prototype != null)
                {
                    materialsFixed = true;

                    if (prototype.IsEmpty)
                    {
                        Debug.Log($"Material correction: No material found to reproduce");
                    }
                    else
                    {
                        Shader shader = Shader.Find("MarmosetUBER");

                        foreach (var data in MaterialResolver(subTransform))
                        {
                            try
                            {
                                var materialAdaptation = new MaterialAdaptation(prototype, data, shader);
                                materialAdaptation.ApplyToTarget(VerboseLogging);

                                adaptations.Add(materialAdaptation);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"Material correction: Adaptation failed for material {data}: {ex}");
                                Debug.LogException(ex);
                            }
                        }
                        Debug.Log($"Material correction: All done. Applied {adaptations.Count} adaptations");
                    }
                }
            }

            if (DateTime.Now > repairMaterialsIn && --repairMaterialsInFrames == 0)
            {
                repairMaterialsIn = DateTime.MaxValue;
                Debug.Log($"Undocked. Resetting materials");
                foreach (MaterialAdaptation adaptation in adaptations)
                    adaptation.PostDockFixOnTarget(VerboseLogging);
            }
        }
    }
}