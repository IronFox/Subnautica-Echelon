using BepInEx;
using HarmonyLib;
using Nautilus.Handlers;
using Subnautica_Echelon.Logs;
using Subnautica_Echelon.Modules;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using VehicleFramework;
using VehicleFramework.Assets;
using VehicleFramework.Patches;

namespace Subnautica_Echelon
{



    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(VehicleFramework.PluginInfo.PLUGIN_GUID, VehicleFramework.PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID, Nautilus.PluginInfo.PLUGIN_VERSION)]
    public class MainPatcher : BaseUnityPlugin
    {
        internal static EchelonConfig PluginConfig { get; private set; }
        internal const string WorkBenchTab = "Storage";
        internal static string RootFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static string ImagesFolder { get; } = Path.Combine(RootFolder, "images");


        public void Awake()
        {
            try
            {
                Logging.Target = new Logs.LoggerTarget(Logger);
                Log.Write($"MainPatcher.Awake()");

                RecipePurger.Purge();


                Echelon.GetAssets();
                Log.Write($"MainPatcher.Awake() done");

            }
            catch (Exception ex)
            {
                PLog.Exception($"MainPatcher.Awake()", ex, gameObject);
            }
        }

        public void Start()
        {
            try
            {
                Log.Write("MainPatcher.Start()");
                LanguageHandler.RegisterLocalizationFolder();
                PluginConfig = OptionsPanelHandler.RegisterModOptions<EchelonConfig>();
                var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.PatchAll();
                UWE.CoroutineHost.StartCoroutine(Register());

                //PDAHandler.AddEncyclopediaEntry("BelugaEncy", "Tech/Vehicles", Language.main.Get("Echelon"), Language.main.Get("Ency"));

                Log.Write("MainPatcher.Start() done");
            }
            catch (Exception ex)
            {
                PLog.Exception("MainPatcher.Start()", ex, gameObject);
            }
        }
        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.EnsureComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }

        public static Atlas.Sprite LoadSprite(string filename)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename);
            Log.Write($"Trying to load sprite from {path}");
            try
            {
                return SpriteHelper.GetSprite(path);
            }
            catch (Exception ex)
            {
                PLog.Exception(nameof(LoadSprite), ex, null);
                return null;
            }
        }
        private static Sprite LoadSpriteRaw(string filename)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename);
            Log.Write($"Trying to load sprite from {path}");
            try
            {
                return SpriteHelper.GetSpriteRaw(path);
            }
            catch (Exception ex)
            {
                PLog.Exception(nameof(LoadSpriteRaw), ex, null);
                return null;
            }
        }

        public IEnumerator Register()
        {
            Coroutine started = null;
            try
            {
                Log.Write("MainPatcher.Register()");
                Log.Write("model loaded: " + Echelon.model.name);
                var sub = Echelon.model.EnsureComponent<Echelon>();
                Log.Write("echelon attached: " + sub.name);

                Echelon.craftingSprite = LoadSprite("images/echelon.png");
                Echelon.pingSprite = LoadSprite("images/outline.png") ?? Echelon.emptySprite;
                Echelon.saveFileSprite = LoadSpriteRaw("images/outline.png");
                Echelon.moduleBackground = LoadSpriteRaw("images/moduleBackground.png");
                started = UWE.CoroutineHost.StartCoroutine(VehicleRegistrar.RegisterVehicle(sub, true));

                TorpedoModule.RegisterAll();
                DriveModule.RegisterAll();
                NuclearBatteryModule.RegisterAll();
                RepairModule.RegisterAll();
                RailgunModule.RegisterAll();

                AudioPatcher.Patcher = (source) => FreezeTimePatcher.Register(source);
                ActorAdapter.IsOutOfWater = (go, pos) =>
                {
                    var wf = go.GetComponent<WorldForces>();
                    return wf.IsAboveWater();
                };
                TargetAdapter.ResolveTarget = (go, rb) =>
                {
                    var mixin = go.GetComponent<LiveMixin>();
                    if (mixin == null)
                        return null;
                    var vehicle = go.GetComponent<Vehicle>();
                    if (vehicle != null)
                        return null;    //don't target vehicles
                    if (go.name.Contains("Cyclops-MainPrefab"))
                        return null;    //don't target cyclops
                    return new Adapters.MixinTargetAdapter(go, rb, mixin);

                };
                RigidbodyPatcher.Patch = (go, rb) =>
                {
                    try
                    {
                        //Log.Write($"Patching rigidbody for {go}");
                        rb.drag = 10f;
                        rb.angularDrag = 10f;
                        rb.useGravity = false;
                        //rb.interpolation = RigidbodyInterpolation.Extrapolate;
                        //rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                        var worldForces = CopyComponent<WorldForces>(SeamothHelper.Seamoth.GetComponent<SeaMoth>().worldForces, go);
                        worldForces.useRigidbody = rb;
                        worldForces.underwaterGravity = 0f;
                        worldForces.aboveWaterGravity = 9.8f;
                        worldForces.waterDepth = 0f;
                        worldForces.lockInterpolation = true;

                        //Log.Write("Rigidbody patched: " + rb);
                    }
                    catch (Exception ex)
                    {
                        PLog.Exception("RigidbodyAdapter.MakeRigidbody", ex, gameObject);
                        throw;
                    }
                };

                SoundAdapter.SoundCreator = new Adapters.FModSoundCreator();

                Log.Write("MainPatcher.Register() done");
            }
            catch (Exception ex)
            {
                PLog.Exception($"MainPatcher.Register()", ex, gameObject);
            }
            yield return started;
        }


    }
}
