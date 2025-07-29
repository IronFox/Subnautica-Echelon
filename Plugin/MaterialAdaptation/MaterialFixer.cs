using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;
using VehicleFramework;

namespace Subnautica_Echelon.MaterialAdaptation
{
    /// <summary>
    /// Logging configuration
    /// </summary>
    public readonly struct LogConfig
    {
        public bool LogMaterialChanges { get; }
        public string Prefix { get; }
        public bool IncludeTimestamp { get; }
        public bool LogExtraSteps { get; }

        public LogConfig(bool logMaterialChanges, string prefix, bool includeTimestamp, bool logExtraSteps)
        {
            LogMaterialChanges = logMaterialChanges;
            Prefix = prefix;
            IncludeTimestamp = includeTimestamp;
            LogExtraSteps = logExtraSteps;
        }

        public const string DefaultPrefix = "Echelon Material Fix";

        public static LogConfig Default { get; } = new LogConfig(
            logMaterialChanges: false,
            prefix: DefaultPrefix,
            includeTimestamp: true,
            logExtraSteps: true
            );

        public static LogConfig Silent { get; } = new LogConfig(
            logMaterialChanges: false,
            prefix: DefaultPrefix,
            includeTimestamp: true,
            logExtraSteps: false
            );

        public static LogConfig Verbose { get; } = new LogConfig(
            logMaterialChanges: true,
            prefix: DefaultPrefix,
            includeTimestamp: true,
            logExtraSteps: true
            );

        public void LogExtraStep(string msg)
        {
            if (!LogExtraSteps)
                return;

            Debug.Log(MakeMessage(msg));
        }

        private string MakeMessage(string msg)
        {
            if (!string.IsNullOrEmpty(Prefix))
            {
                if (IncludeTimestamp)
                    return $"{DateTime.Now:HH:mm:ss.fff} {Prefix}: {msg}";
                return $"{Prefix}: {msg}";
            }
            else
            {
                if (IncludeTimestamp)
                    return $"{DateTime.Now:HH:mm:ss.fff} {msg}";
                return msg;
            }
        }


        public void LogWarning(string msg)
        {
            Debug.LogWarning(MakeMessage(msg));
        }

        public void LogError(string msg)
        {
            Debug.LogError(MakeMessage(msg));
        }

        public void LogMaterialChange(string msg)
        {
            if (!LogMaterialChanges)
                return;
            Debug.Log(MakeMessage(msg));
        }
        public void LogMaterialChange(Func<string> msg)
        {
            if (!LogMaterialChanges)
                return;
            Debug.Log(MakeMessage(msg()));
        }

        private string ValueToString<T>(T value)
        {
            if (value is float f0)
                return f0.ToString(CultureInfo.InvariantCulture);
            return value?.ToString();
        }

        public void LogMaterialVariableSet<T>(
            ShaderPropertyType type,
            string name,
            T old,
            T value,
            Material m)
        {
            if (LogMaterialChanges)
                Debug.Log(MakeMessage($"Setting {type} {name} ({ValueToString(old)} -> {ValueToString(value)}) on material {m}"));
        }
    }


    /// <summary>
    /// Helper class to fix materials automatically. Should be instantiated on the vehicle
    /// you wish to fix materials of
    /// </summary>
    /// <author>https://github.com/IronFox</author>
    public class MaterialFixer
    {

        private float repairMaterialsInSeconds = float.MaxValue;
        private bool doRepairMaterialsPostUndock;
        private int repairMaterialsInFrames = 3;
        private bool materialsFixed;
        private readonly List<MaterialAdaptation> adaptations = new List<MaterialAdaptation>();

        public bool MaterialsAreFixed => materialsFixed;

        public ModVehicle Vehicle { get; }

        /// <summary>
        /// Controls how debug logging should be performed
        /// </summary>
        public LogConfig LogConfig { get; set; }

        public Func<IEnumerable<SurfaceShaderData>> MaterialResolver { get; }

        /// <summary>
        /// Null or in [0,1].<br/>
        /// If non-null, enforces the same uniform shininess level on all materials
        /// </summary>
        public float? UniformShininess { get; set; }
        private float? uniformShininess;

        /// <summary>
        /// Constructs the instance
        /// </summary>
        /// <param name="owner">Owning vehicle</param>
        /// <param name="materialResolver">The solver function to fetch all materials to translate.
        /// If null, a default implementation is used which 
        /// mimics VF's default material selection in addition to filtering out non-standard materials</param>
        /// <param name="logConfig">Log Configuration. If null, defaults to <see cref="LogConfig.Default" /></param>
        public MaterialFixer(
            ModVehicle owner,
            LogConfig? logConfig = null,
            Func<IEnumerable<SurfaceShaderData>> materialResolver = null
            )
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            Vehicle = owner;
            LogConfig = logConfig ?? LogConfig.Default;
            MaterialResolver = materialResolver ?? (() => DefaultMaterialResolver(owner, LogConfig));
        }

        /// <summary>
        /// Default material address resolver function. Can be modified to also return materials with divergent shader names
        /// </summary>
        /// <param name="vehicle">Owning vehicle</param>
        /// <param name="ignoreShaderNames">True to return all materials, false to only return Standard materials</param>
        /// <param name="logConfig">Log Configuration</param>
        /// <returns>Enumerable of all loaded surface shader data sets</returns>
        public static IEnumerable<SurfaceShaderData> DefaultMaterialResolver(ModVehicle vehicle, LogConfig logConfig, bool ignoreShaderNames = false)
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
                    UnityEngine.Object.DestroyImmediate(renderer.gameObject.GetComponent<Skybox>());
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

                //enumerate all materials:

                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    var material = SurfaceShaderData.From(renderer, i, logConfig, ignoreShaderNames);
                    if (material != null)
                        yield return material;
                }
            }
        }

        /// <summary>
        /// Notifies that the vehicle has just undocked from a docking bay (moonpool, etc).
        /// </summary>
        /// <remarks>Should be called from your vehicle OnVehicleUndocked() method</remarks>
        public void OnVehicleUndocked()
        {
            repairMaterialsInSeconds = 0.2f;
            repairMaterialsInFrames = 1;
            doRepairMaterialsPostUndock = true;
        }

        /// <summary>
        /// Notifies that the vehicle has just docked to a docking bay (moonpool, etc).
        /// </summary>
        /// <remarks>Should be called from your vehicle OnVehicleDocked() method</remarks>
        public void OnVehicleDocked() => OnVehicleUndocked();

        /// <summary>
        /// Forcefully reapplies all material adaptations.
        /// Normally not necessary
        /// </summary>
        public void ReApply()
        {
            foreach (MaterialAdaptation adaptation in adaptations)
                adaptation.ApplyToTarget(LogConfig);
        }

        /// <summary>
        /// Fixes materials if necessary/possible.
        /// Also fixes undock material changes if <see cref="OnVehicleUndocked"/> was called before
        /// </summary>
        /// <remarks>Should be called from your vehicle Update() method</remarks>
        /// <param name="subTransform">Root transform of your sub</param>
        public bool OnUpdate()
        {
            bool anyChanged = false;

            if (!materialsFixed)
            {
                var prototype = MaterialPrototype.FromSeamoth(LogConfig);

                if (prototype != null)
                {
                    materialsFixed = true;
                    uniformShininess = UniformShininess;

                    if (prototype.IsEmpty)
                    {
                        LogConfig.LogError($"No material prototype found on Seamoth");
                    }
                    else
                    {

                        try
                        {
                            var prefabId = Vehicle.GetComponent<PrefabIdentifier>();
                            if (prefabId != null)
                                LogConfig.LogExtraStep($"Fixing materials for {prefabId.Id} {Vehicle.GetName()} on {Vehicle.gameObject.GetInstanceID()}");
                            else
                                LogConfig.LogExtraStep($"Fixing materials for {Vehicle.GetName()} on {Vehicle.gameObject.GetInstanceID()}");
                        }
                        catch (Exception ex)
                        { }


                        Shader shader = Shader.Find("MarmosetUBER");

                        foreach (var data in MaterialResolver())
                        {
                            try
                            {
                                var materialAdaptation = new MaterialAdaptation(prototype, data, shader);
                                materialAdaptation.ApplyToTarget(LogConfig, uniformShininess);

                                adaptations.Add(materialAdaptation);
                            }
                            catch (Exception ex)
                            {
                                LogConfig.LogError($"Adaptation failed for material {data}: {ex}");
                                Debug.LogException(ex);
                            }
                        }
                        LogConfig.LogExtraStep($"All done. Applied {adaptations.Count} adaptations");
                    }
                }
                anyChanged = true;
            }
            else if (uniformShininess != UniformShininess)
            {
                uniformShininess = UniformShininess;
                foreach (MaterialAdaptation adaptation in adaptations)
                {
                    adaptation.ApplyToTarget(LogConfig, uniformShininess);
                }
                anyChanged = true;
            }
            if (doRepairMaterialsPostUndock)
            {
                repairMaterialsInSeconds -= Time.deltaTime;
                if (repairMaterialsInSeconds < 0 && --repairMaterialsInFrames == 0)
                {
                    repairMaterialsInSeconds = float.MaxValue;
                    doRepairMaterialsPostUndock = false;
                    LogConfig.LogExtraStep($"Undocked. Resetting {adaptations.Count} materials");
                    foreach (MaterialAdaptation adaptation in adaptations)
                        adaptation.PostDockFixOnTarget(LogConfig);
                    anyChanged = true;
                }
                else
                    LogConfig.LogExtraStep($"Undock repair in progress: {repairMaterialsInSeconds:F2} seconds left, {repairMaterialsInFrames} frames left");
            }

            return anyChanged;
        }
    }
}