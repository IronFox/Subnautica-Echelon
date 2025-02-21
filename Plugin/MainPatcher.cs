using BepInEx;
using FMODUnity;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Utility;
using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UWE;
using VehicleFramework;
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
        internal static EchelonConfig Config { get; private set; }


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
                Config = OptionsPanelHandler.RegisterModOptions<EchelonConfig>();
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
                started = UWE.CoroutineHost.StartCoroutine(VehicleRegistrar.RegisterVehicle(sub,true));

                AudioPatcher.Patcher = (source) => FreezeTimePatcher.Register(source);
                RigidbodyAdapter.MakeRigidbody = (go, mass) =>
                {
                    try
                    {
                        Log.Write($"Creating rigidbody for {go} with mass {mass}");
                        var rb = go.EnsureComponent<Rigidbody>();
                        rb.mass = mass;
                        rb.drag = 10f;
                        rb.angularDrag = 10f;
                        rb.useGravity = false;

                        var worldForces = CopyComponent<WorldForces>(SeamothHelper.Seamoth.GetComponent<SeaMoth>().worldForces, go);
                        worldForces.useRigidbody = rb;
                        worldForces.underwaterGravity = 0f;
                        worldForces.aboveWaterGravity = 9.8f;
                        worldForces.waterDepth = 0f;

                        Log.Write("Rigidbody created: " + rb);
                        return rb;
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
