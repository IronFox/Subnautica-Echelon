using BepInEx;
using FMODUnity;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;
using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UWE;
using VehicleFramework;
using VehicleFramework.Assets;
using VehicleFramework.Patches;
using VehicleFramework.VehicleTypes;
using static OVRHaptics;

namespace Subnautica_Echelon
{



    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(VehicleFramework.PluginInfo.PLUGIN_GUID)]
    [BepInDependency(Nautilus.PluginInfo.PLUGIN_GUID, Nautilus.PluginInfo.PLUGIN_VERSION)]
    public class MainPatcher : BaseUnityPlugin
    {
        internal static EchelonConfig PluginConfig { get; private set; }


        public void Awake()
        {
            try
            {
                Log.Write($"MainPatcher.Awake()");
                Echelon.GetAssets();
                Log.Write($"MainPatcher.Awake() done");

            }
            catch (Exception ex)
            {
                Log.Write($"MainPatcher.Awake()", ex);
            }
        }

        public void Start()
        {
            try
            {
                Log.Write("MainPatcher.Start()");
                PluginConfig = OptionsPanelHandler.RegisterModOptions<EchelonConfig>();
                var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                harmony.PatchAll();
                UWE.CoroutineHost.StartCoroutine(Register());
                Log.Write("MainPatcher.Start() done");
            }
            catch (Exception ex)
            {
                Log.Write("MainPatcher.Start()", ex);
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

        private static Atlas.Sprite LoadSprite(string filename)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),filename);
            Log.Write($"Trying to load sprite from {path}");
            try
            {
                return SpriteHelper.GetSprite(path);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return null;
            }
        }

        public IEnumerator Register()
        {
            Coroutine started = null;
            try
            {
                Log.Write("MainPatcher.Register()");
                Log.Write("");
                Log.Write("model loaded: " + Echelon.model.name);
                var sub = Echelon.model.EnsureComponent<Echelon>();
                Log.Write("echelon attached: "+sub.name);

                Echelon.craftingSprite = LoadSprite("echelon.png");
                Echelon.pingSprite = LoadSprite("outline.png") ?? Echelon.emptySprite;
                started = UWE.CoroutineHost.StartCoroutine(VehicleRegistrar.RegisterVehicle(sub,true));


                AudioPatcher.Patcher = (source) => FreezeTimePatcher.Register(source);
                //ExplosionAdapter.HandleExplosion = (exp) =>
                //{
                //    WorldForces.AddExplosion(exp.Center, DayNightCycle.main.timePassed, exp.Magnitude, exp.Radius);
                //};
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
                    return new MixinTargetAdapter(go, rb, mixin);

                };
                RigidbodyPatcher.Patch = (go, rb) =>
                {
                    try
                    {
                        Log.Write($"Patching rigidbody for {go}");
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

                        Log.Write("Rigidbody patched: " + rb);
                    }
                    catch (Exception ex)
                    {
                        Log.Write("RigidbodyAdapter.MakeRigidbody", ex);
                        throw;
                    }
                };

                SoundAdapter.SoundCreator = new FModSoundCreator();

                Log.Write("MainPatcher.Register() done");
            }
            catch (Exception ex)
            {
                Log.Write($"MainPatcher.Register()", ex);
            }
            yield return started;
        }


    }
}
