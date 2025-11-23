using Subnautica_Echelon.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        private string? ValueToString<T>(T value)
        {
            if (value is float f0)
                return f0.ToString(CultureInfo.InvariantCulture);
            return value?.ToString();
        }

        public void LoggedMaterialUpdate<T>(
            ShaderPropertyType type,
            string name,
            Func<T> getOld,
            T newValue,
            Action<T> setNew,
            Func<T, T, bool> equals,
            Material m)
        {
            try
            {
                var old = getOld();
                if (equals(old, newValue))
                    return;
                LogMaterialVariableSet(type, name, old, newValue, m);
                setNew(newValue);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                LogError($"Failed to set color {name} ({newValue}) on material {m}");
            }
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
        private static Dictionary<EchelonControl, MaterialFixer> AllInstances { get; } = [];
        public bool MaterialsAreFixed => materialsFixed;

        public Echelon Vehicle { get; }

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
        public SkyApplier? SkyApplier { get; private set; }
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
            Echelon owner,
            LogConfig? logConfig = null,
            Func<IEnumerable<SurfaceShaderData>>? materialResolver = null
            )
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            Vehicle = owner;
            LogConfig = logConfig ?? LogConfig.Default;
            MaterialResolver = materialResolver ?? (() => DefaultMaterialResolver(owner, LogConfig));

        }

        public void Awake()
        {
            AllInstances.Add(Vehicle.Control, this);
        }

        public void Destroy()
        {
            AllInstances.Remove(Vehicle.Control);

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
            repairMaterialsInSeconds = 0.5f;
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
                        catch (Exception)
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

            if (anyChanged)
            {
                RefreshSkyApplier();
            }
            //Vehicle.GetComponent<SkyApplier>().RefreshDirtySky();

            return anyChanged;
        }


        private void RefreshSkyApplier()
        {
            try
            {
                if (SkyApplier == null)
                {
                    var skyApplier = Vehicle.GetComponent<SkyApplier>();
                    if (skyApplier != null)
                    {
                        PLog.Write($"Destroying sky applier {skyApplier.NiceName()}");
                        UnityEngine.Object.Destroy(skyApplier);
                    }

                    SkyApplier = Vehicle.gameObject.AddComponent<SkyApplier>();
                }

                LogConfig.LogExtraStep($"Updating sky applier renderers");
                SkyApplier.renderers = [.. adaptations.Select(x => x.Target.Renderer)];
                SkyApplier.dynamic = true;
                LogConfig.LogExtraStep($"Rebuilt sky applier with {SkyApplier.renderers.Length} renderers");
            }
            catch (Exception ex)
            {
                LogConfig.LogError($"Failed to refresh SkyApplier: {ex}");
                Debug.LogException(ex);
            }
        }

        internal static void UpdateMainTexture(EchelonControl echelon, Renderer renderer, int materialIndex, Texture newMainTex)
        {
            if (AllInstances.TryGetValue(echelon, out var fixer))
            {
                int found = -1;
                for (int i = 0; i < fixer.adaptations.Count; i++)
                {
                    var adaptation = fixer.adaptations[i];
                    if (adaptation.Target.Renderer == renderer && adaptation.Target.MaterialIndex == materialIndex)
                    {
                        found = i;
                        break;
                    }
                }

                if (found >= 0)
                {
                    //PLog.Write($"MaterialFixer: Updating main texture for renderer {renderer.NiceName()} #{materialIndex} to {newMainTex.NiceName()}");
                    var adapt = fixer.adaptations[found] = fixer.adaptations[found].WithNewMainTexture(newMainTex);
                    adapt.ApplyToTarget(fixer.LogConfig, fixer.uniformShininess);
                }
                else
                    PLog.Warn($"MaterialFixer: Could not find adaptation for renderer {renderer} index {materialIndex} to signal texture shininess change");
            }
        }

        internal static void UpdateColorSmoothness(EchelonControl echelon, Renderer renderer, int materialIndex, Color color, float newSmoothness)
        {
            throw new NotImplementedException();
            if (AllInstances.TryGetValue(echelon, out var fixer))
            {
                int found = -1;
                for (int i = 0; i < fixer.adaptations.Count; i++)
                {
                    var adaptation = fixer.adaptations[i];
                    if (adaptation.Target.Renderer == renderer && adaptation.Target.MaterialIndex == materialIndex)
                    {
                        found = i;
                        break;
                    }
                }

                if (found >= 0)
                {
                    //PLog.Write($"MaterialFixer: Updating color smoothness for renderer {renderer.NiceName()} #{materialIndex} to {color}/{newSmoothness}");
                    var adapt = fixer.adaptations[found] = fixer.adaptations[found].WithNewColorSmoothness(color, newSmoothness);
                    adapt.ApplyToTarget(fixer.LogConfig, fixer.uniformShininess);
                }
                else
                    PLog.Warn($"MaterialFixer: Could not find adaptation for renderer {renderer} index {materialIndex} to update smoothness value");
            }
        }
    }
}