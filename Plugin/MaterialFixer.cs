using System;
using System.Collections.Generic;
using UnityEngine;
using VehicleFramework;

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

        public ModVehicle Vehicle { get; }
        public bool VerboseLogging { get; set; }

        public Func<IEnumerable<SurfaceShaderData>> MaterialResolver { get; }

        /// <summary>
        /// Constructs the instance
        /// </summary>
        /// <param name="materialResolver">The solver function to fetch all materials to translate.
        /// If null, a default implementation is used which 
        /// mimics VF's default material selection in addition to filtering out non-standard </param>
        /// <param name="verbose">Log verbosely. Can be changed any time</param>
        public MaterialFixer(
            ModVehicle owner,
            Func<IEnumerable<SurfaceShaderData>> materialResolver = null,
            bool verbose=false
            )
        { 
            Vehicle = owner;
            VerboseLogging = verbose;
            MaterialResolver = materialResolver ?? (() => DefaultMaterialResolver(owner));
        }

        /// <summary>
        /// Default material address resolver function. Can be modified to also return materials with divergent shader names
        /// </summary>
        /// <param name="vehicle">Owning vehicle</param>
        /// <param name="ignoreShaderNames">True to return all materials, false to only return Standard materials</param>
        /// <returns>Enumerable of all suitable material addresses</returns>
        public static IEnumerable<SurfaceShaderData> DefaultMaterialResolver(ModVehicle vehicle, bool ignoreShaderNames=false)
        {
            var renderers = vehicle.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                // copied from VF default behavior:

                // skip some materials
                if (renderer.gameObject.GetComponent<Skybox>())
                {
                    // I feel okay using Skybox as the designated "don't apply marmoset to me" component.
                    // I think there's no reason a vehicle should have a skybox anywhere.
                    // And if there is, I'm sure that developer can work around this.
                    Component.DestroyImmediate(renderer.gameObject.GetComponent<Skybox>());
                    continue;
                }
                if (renderer.gameObject.name.ToLower().Contains("light"))
                {
                    continue;
                }
                if (vehicle.CanopyWindows != null && vehicle.CanopyWindows.Contains(renderer.gameObject))
                {
                    continue;
                }

                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    var material = SurfaceShaderData.From(renderer, i, ignoreShaderNames);
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
        public void OnUpdate()
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

                        foreach (var data in MaterialResolver())
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