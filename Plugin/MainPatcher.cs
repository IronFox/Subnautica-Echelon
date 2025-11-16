using BepInEx;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility.ModMessages;
using Newtonsoft.Json;
using Subnautica_Echelon.Logs;
using Subnautica_Echelon.MaterialAdaptation;
using Subnautica_Echelon.Modules;
using Subnautica_Echelon.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VehicleFramework.Admin;
using VehicleFramework.Assets;
using VehicleFramework.Patches;

namespace Subnautica_Echelon
{



    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(VehicleFramework.PluginInfo.PLUGIN_GUID, VehicleFramework.PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID, Nautilus.PluginInfo.PLUGIN_VERSION)]
    public class MainPatcher : BaseUnityPlugin
    {
        private static EchelonConfig? config;
        internal static EchelonConfig PluginConfig => config ?? throw new InvalidOperationException("Config not loaded yet");
        internal const string WorkBenchTab = "Storage";
        internal static string RootFolder { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static string ImagesFolder { get; } = Path.Combine(RootFolder, "images");


        /// <inheritdoc/>
        public void LoadLanguagesAndConfig(string languageFolderName = "Localization")
        {
            var aType = typeof(BindableButtonAttribute);

            var languageFolder = Path.Combine(Path.GetDirectoryName(typeof(EchelonConfig).Assembly.Location), languageFolderName);

            PLog.Write($"Registering localization folder at '{languageFolder}'");

            LanguageHandler.RegisterLocalizationFolder(languageFolder);

            if (!Directory.Exists(languageFolder))
            {
                PLog.Warn($"Language folder '{languageFolder}' does not exist. Skipping language loading.");
            }

            config = OptionsPanelHandler.RegisterModOptions<EchelonConfig>();

            var languages = Directory.Exists(languageFolder) ? new DirectoryInfo(languageFolder).GetFiles("*.json")
                .Select(lang =>
                {
                    var l = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        File.ReadAllText(lang.FullName)
                        );
                    return (Name: Path.GetFileNameWithoutExtension(lang.Name), Dict: l);
                }).ToList()
                : [];


            foreach (var f in typeof(EchelonConfig).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                var b = Attribute.GetCustomAttribute(f, aType) as BindableButtonAttribute;
                if (b is null)
                    continue;
                try
                {

                    if (EnumHandler.TryAddEntry<GameInput.Button>($"Echelon.Button.{f.Name}", out var builder))
                    {
                        bool anyTranslation = false;
                        if (!string.IsNullOrEmpty(b.LabelLocalizationKey) && languages.Count > 0)
                        {
                            foreach (var lang in languages)
                            {
                                lang.Dict.TryGetValue(b.LabelLocalizationKey!, out var translated);
                                string? toolTipTranslated = null;
                                if (!string.IsNullOrEmpty(b.TooltipLocalizationKey))
                                    lang.Dict.TryGetValue(b.TooltipLocalizationKey!, out toolTipTranslated);
                                if (!string.IsNullOrEmpty(translated))
                                {
                                    builder = builder.CreateInput(displayName: translated, tooltip: toolTipTranslated ?? "", language: lang.Name);
                                    anyTranslation = true;
                                }
                            }
                        }

                        if (!anyTranslation)
                            builder = builder.CreateInput(b.Name);

                        if (!string.IsNullOrEmpty(b.KeyboardDefault))
                            builder = builder.WithKeyboardBinding(b.KeyboardDefault);
                        else
                            builder = builder.WithKeyboardBinding("None");
                        if (!string.IsNullOrEmpty(b.GamepadDefault))
                            builder = builder.WithControllerBinding(b.GamepadDefault);
                        else
                            builder = builder.WithControllerBinding("None");

                        builder.WithCategory("Echelon");
                        f.SetValue(config, builder.Value);
                    }
                }
                catch (Exception ex)
                {
                    PLog.Fail($"Error registering bindable button for field '{f.Name}': {ex}");
                }
            }

        }


        public void Awake()
        {
            try
            {
                Logging.Target = new Logs.LoggerTarget(Logger);
                Log.Write($"MainPatcher.Awake()");

                RecipePurger.Purge();


                Echelon.GetAssets();


                ModMessageSystem.SendGlobal("FindMyUpdates", "https://raw.githubusercontent.com/IronFox/Subnautica-Echelon/refs/heads/main/mod-info.json");

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
                LoadLanguagesAndConfig();
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
            return (T)copy;
        }

        public static Sprite? LoadSprite(string filename)
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
        private static Sprite? LoadSpriteRaw(string filename)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename);
            Log.Write($"Trying to load sprite from {path}");
            try
            {
                return SpriteHelper.GetSprite(path);
            }
            catch (Exception ex)
            {
                PLog.Exception(nameof(LoadSpriteRaw), ex, null);
                return null;
            }
        }

        public IEnumerator Register()
        {
            Coroutine? started = null;
            try
            {
                Log.Write("MainPatcher.Register()");
                Log.Write("model loaded: " + Echelon.model!.name);
                var sub = Echelon.model.EnsureComponent<Echelon>();
                Log.Write("echelon attached: " + sub.name);

                Echelon.craftingSprite = LoadSprite("images/echelon.png");
                Echelon.pingSprite = LoadSprite("images/outline.png") ?? Echelon.emptySprite;
                Echelon.saveFileSprite = LoadSpriteRaw("images/outline.png");
                Echelon.moduleBackground = LoadSpriteRaw("images/moduleBackground.png");
                started = UWE.CoroutineHost.StartCoroutine(VehicleRegistrar.RegisterVehicle(sub, true));

                MaterialAdapter.UpdateMainTexture = MaterialFixer.UpdateMainTexture;
                MaterialAdapter.UpdateColorSmoothness = MaterialFixer.UpdateColorSmoothness;

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

                        var worldForces = CopyComponent<WorldForces>(SeamothHelper.Seamoth!.GetComponent<SeaMoth>().worldForces, go);
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
