using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using VehicleFramework;
using VehicleFramework.Engines;
using VehicleFramework.VehicleParts;
using VehicleFramework.VehicleTypes;
using UnityEngine.Networking;
using Nautilus.Extensions;
using static Vehicle;
using UnityEngine.Experimental.GlobalIllumination;


namespace Subnautica_Echelon
{

    public class VoidEngine : ModVehicleEngine
    {
        public override void ExecutePhysicsMove()
        { }
        public override void ControlRotation()
        { }
        public override void DrainPower(Vector3 moveDirection)
        {
            //todo
        }
    }


    public class Method1<T>
    {
        public Method1(MethodInfo method, Component control)
        {
            Method = method;
            Control = control;
        }


        public void Invoke(T p)
        {
            try
            {
                Method.Invoke(Control, new object[] { p });
            }
            catch (Exception ex)
            {
                Log.Write("Caught exception while trying to invoke " + Method.Name + " on " + Control.name);
                Log.Write(ex);
            }
        }

        public MethodInfo Method { get; }
        public Component Control { get; }
    }

    public class FieldAccess<T>
    {
        private T lastValue;
        public FieldAccess(FieldInfo propertyInfo, Component control)
        {
            Field = propertyInfo;
            Control= control;
            lastValue = Get();
            Log.Write($"Initial value of {Field.Name} of {Control.name} determined: {lastValue}");
        }

        public T Get() => (T)Field.GetValue(Control);
        public void Set(T value)
        {
            if (!value.Equals(lastValue))
            {
                Log.Write($"Updating {Field.Name} of {Control.name} {lastValue} -> {value}");
                Field.SetValue(Control, value);
                lastValue = value;
            }
        }

        public FieldInfo Field { get; }
        public Component Control { get; }
    }


    public static class Log
    {
        public static void Write(string message)
        {
            File.AppendAllText(@"C:\Temp\Logs\log.txt", $"{DateTimeOffset.Now:HH:mm:ss.fff} {message}\r\n");

        }

        public static void Write(Exception ex)
        {
            Write(ex.GetType().Name);
            Write(ex.Message);
            Write(ex.StackTrace);
        }
        public static void Write(string whileDoing, Exception caughtException)
        {
            Write($"Caught exception during {whileDoing}");
            Write(caughtException);
        }
    }


    public class Echelon : Submersible
    {
        public static GameObject model;
        private EchelonControl control;

        public static void GetAssets()
        {
            try
            {
                Log.Write("Echelon.GetAssets()");
                var modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var bundlePath = Path.Combine(modPath, "echelon");
                Log.Write($"Trying to load asset bundle from '{bundlePath}'");
                if (!File.Exists(bundlePath))
                    Log.Write("This file does not appear to exist");
                var bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle != null)
                {
                    var assets = bundle.LoadAllAssets();
                    foreach (var obj in assets)
                    {
                        Log.Write("Scanning object: " + obj.name);
                        if (obj.name == "Echelon")
                        {
                            model = (GameObject)obj;
                        }
                    }
                    if (model == null)
                        Log.Write("Model not found among: " + string.Join(", ", Helper.Names(assets)));
                }
                else
                    Log.Write("Unable to loade bundle from path");
                Log.Write("Echelon.GetAssets() done");
            }
            catch (Exception ex)
            {
                Log.Write("Echelon.GetAssets()", ex);
            }
        }


        private FieldAccess<T> RequireField<T>(Component control, string propertyName)
        {
            var t = control.GetType();
            var p = t.GetField(propertyName);
            if (p == null)
                throw new KeyNotFoundException($"Unable to find field {propertyName} on instance {control.name} of type {t.Name}. Found fields are " + string.Join(",", Helper.Names(t.GetFields())));
            return new FieldAccess<T>(p, control);
        }


        private Method1<T> RequireMethod1<T>(Component control, string methodName)
        {
            var t = control.GetType();
            var p = t.GetMethod(methodName);
            if (p == null)
                throw new KeyNotFoundException($"Unable to find method {methodName} on instance {control.name} of type {t.Name}");
            return new Method1<T>(p, control);
        }

        private bool isInitialized = false;
        private void LocalInit()
        {
            if (!isInitialized)
            {
                Log.Write($"LocalInit() first time");
                isInitialized = true;
                try
                {
                    Engine = new VoidEngine();
                    CanLeviathanGrab = false;
                    stabilizeRoll = false;
                    sidewaysTorque = 0;
                    sidewardForce = 0;
                    forwardForce = 0;
                    backwardForce = 0;
                    verticalForce = 0;
                    onGroundForceMultiplier = 0;



                    control = GetComponent<EchelonControl>();
                    if (control != null)
                    {
                        Log.Write("Found control");
                        control.isBoarded = false;

                    }
                    else
                    {
                        if (transform == null)
                            Log.Write($"Do not have a transform");
                        else
                        {
                            Log.Write($"This is {transform.name}");
                            Log.Write("This has components: " + Helper.NamesS(Helper.AllComponents(transform)));
                            Log.Write("This has children: " + Helper.NamesS(Helper.Children(transform)));
                        }
                    }
                    Log.Write($"LocalInit() done");

                }
                catch (Exception e)
                {
                    Log.Write("LocalInit()",e);
                }

            }
        }

        public override void Start()
        {
            try
            {
                Log.Write("Echelon.Start()");
                LocalInit();

                base.Start();
                Log.Write("Echelon.Start() done");

            }
            catch (Exception ex)
            {
                Log.Write("Echelon.Start()", ex);
            }
        }

        public override void PlayerEntry()
        {
            try
            {
                Log.Write("Echelon.PlayerEntry()");
                LocalInit();

                base.PlayerEntry();
                control.Onboard(Player.main.transform);
            }
            catch (Exception ex)
            {
                Log.Write("Echelon.PlayerEntry()", ex);
            }
        }

        public override void PlayerExit()
        {
            try
            {
                Log.Write("Echelon.PlayerExit()");
                LocalInit();
                base.PlayerExit();
                control.Offboard();
            }
            catch (Exception ex)
            {
                Log.Write("Echelon.PlayerExit()", ex);
            }
        }

        private bool fixedUpdateError = false;

        public override void FixedUpdate()
        {
            try
            {
                LocalInit();
                stabilizeRoll = false;
                base.FixedUpdate();
            }
            catch (Exception ex)
            {
                if (!fixedUpdateError)
                {
                    fixedUpdateError = true;
                    Log.Write("Echelon.FixedUpdate()", ex);
                }
            }
        }


        public override void Update()
        {
            try
            {
                LocalInit();

                control.outOfWater = !GetIsUnderwater();
                var move = GameInput.GetMoveDirection();
                control.forwardAxis =
                    GameInput.GetAnalogValueForButton(GameInput.Button.MoveForward)
                    - GameInput.GetAnalogValueForButton(GameInput.Button.MoveBackward)
                    ;
                control.rightAxis =
                    GameInput.GetAnalogValueForButton(GameInput.Button.MoveRight)
                    - GameInput.GetAnalogValueForButton(GameInput.Button.MoveLeft)
                    ;
                control.upAxis = 
                    GameInput.GetAnalogValueForButton(GameInput.Button.MoveUp)
                    - GameInput.GetAnalogValueForButton(GameInput.Button.MoveDown)
                    ;

                control.freeCamera =
                    GameInput.GetAnalogValueForButton(GameInput.Button.RightHand) > 0.5f
                    ;

                control.overdriveActive =
                    GameInput.GetAnalogValueForButton(GameInput.Button.Sprint) > 0.5f
                    ;

                if (transform.position.y >= Ocean.GetOceanLevel() - 10)
                    control.positionCameraBelowSub = true;
                else if (transform.position.y < Ocean.GetOceanLevel() - 15)
                    control.positionCameraBelowSub = false;

                control.isDocked = docked;

                base.Update();
            }
            catch (Exception ex)
            {
                Log.Write("Echelon.Update()", ex);
            }
        }

        public override int MaxHealth => 10000;
        public override int NumModules => 0;
        public override int BaseCrushDepth => 10000;

        public override string vehicleDefaultName => "Echelon";

        public override VehiclePilotSeat PilotSeat
        {
            get
            {
                var mainSeat = transform.Find("PilotSeat");
                if (!mainSeat)
                {
                    Log.Write("PilotSeat not found");
                    return default;
                }
                return new VehiclePilotSeat
                {
                    Seat = mainSeat.gameObject,
                    SitLocation = mainSeat.gameObject,
                    LeftHandLocation = mainSeat,
                    RightHandLocation = mainSeat,
                };
            }
        }




        public override List<VehicleHatchStruct> Hatches
        {
            get
            {
                var hatch = transform.Find("Hatch");
                if (!hatch)
                {
                    Log.Write("Hatch not found");
                    return new List<VehicleHatchStruct>();
                }
                return new List<VehicleHatchStruct>
                {
                    new VehicleHatchStruct {
                        Hatch = hatch.gameObject,
                        ExitLocation = hatch.Find("ExitLocation"),
                        SurfaceExitLocation= hatch.Find("ExitLocation"),
                    }
                };
            }
        }

        public override GameObject VehicleModel => model;

        public override GameObject CollisionModel => transform.Find("CollisionModel").gameObject;
        public override GameObject BoundingBox => transform.Find("BoundingBox").gameObject;
        public override PilotingStyle pilotingStyle => PilotingStyle.Other;

        public override List<GameObject> WaterClipProxies
        {
            get
            {
                var clipProxyParent = transform.Find("WaterClipProxy");
                var rs = new List<GameObject>();
                if (clipProxyParent != null)
                {
                    for (int i = 0; i < clipProxyParent.childCount; i++)
                        rs.Add(clipProxyParent.GetChild(i).gameObject);
                }
                else
                    Log.Write("Clip proxy not found");
                return rs;
            }
        }

        public override List<VehicleUpgrades> Upgrades
        {
            get
            {
                var rs = new List<VehicleUpgrades>();
                var ui = transform.Find("UpgradesInterface");
                if (ui != null)
                {
                    rs.Add(new VehicleUpgrades
                    {
                        Interface = ui.gameObject,
                        Flap = ui.gameObject,
                    });
                }
                else
                    Log.Write($"Upgrades interface not found");
                return rs;

            }

        }

        public override List<VehicleFloodLight> HeadLights
        {
            get
            {
                var rs = new List<VehicleFloodLight>();
                try
                {
                    var headlights = transform.Find("lights_parent/headlights");
                    if (headlights != null)
                    {
                        for (int i = 0; i < headlights.childCount; i++)
                        {
                            var lt = headlights.GetChild(i);
                            var light = lt.GetComponentInChildren<Light>();
                            rs.Add(new VehicleFloodLight
                            {
                                Angle = light.spotAngle,
                                Color = light.color,
                                Intensity = light.intensity,
                                Light = lt.gameObject,
                                Range = light.range
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("HeadLights", ex);
                }
                Log.Write($"Returning {rs.Count} headlight(s)");
                return rs;

            }

        }
    }
}
