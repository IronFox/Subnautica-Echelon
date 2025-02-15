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
using System.Linq;


namespace Subnautica_Echelon
{
    public class EchelonEngine : ModVehicleEngine
    {
        private MyLogger Log { get; }
        public EchelonEngine()
        {
            Log = new MyLogger(this);
            //AngularDrag = 10;
            
        }

        public float overdriveActive;

        public override void Start()
        {
            base.Start();
        }
        public override void ControlRotation()
        {
        }

        //public override void KillMomentum()
        //{
        //    Log.WriteLowFrequency(MyLogger.Channel.Three, ($"KillMomentum()"));
        //    RB.velocity = Vector3.zero;
        //}

        protected override void MoveWithInput(Vector3 moveInput)
        {
            //Log.WriteLowFrequency(MyLogger.Channel.Two, $"MoveWithInput({moveInput})");

            moveInput = new Vector3(moveInput.x, moveInput.y , moveInput.z * (1.5f + 3 * overdriveActive));

            RB.AddRelativeForce(moveInput, ForceMode.VelocityChange);
        }

        //public override void ExecutePhysicsMove()
        //{
        //    Log.WriteLowFrequency(MyLogger.Channel.Five, $"Move()");
        //    //base.RB.AddForce(base.MV.transform.forward * 1e10f * Time.fixedDeltaTime, ForceMode.VelocityChange);
        //    base.RB.AddForce(base.MV.transform.forward * 10000f * Time.fixedDeltaTime, ForceMode.VelocityChange);
        //    Log.WriteLowFrequency(MyLogger.Channel.Six, $"Velocity = {RB.velocity}");
        //}

        //public void Update()
        //{
        //    if (!CanMove())
        //        Log.WriteLowFrequency(MyLogger.Channel.One, $"Cannot move");
        //    else if (!CanTakeInputs())
        //        Log.WriteLowFrequency(MyLogger.Channel.One, $"Cannot take inputs");
        //    else
        //        Log.WriteLowFrequency(MyLogger.Channel.One, $"All go");
        //}
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
        public static string PathOf(Transform t)
        {
            var parts = new List<string>();
            while (t != null && t.parent != t)
            {
                parts.Add($"{t.name}[{t.GetInstanceID()}]");
                t = t.parent;
            }
            parts.Reverse();
            return string.Join( "/", parts );

        }
        public static string PathOf(Component c)
        {
            return PathOf(c.transform) + $":{c.name}[{c.GetInstanceID()}]({c.GetType()})";
        }
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

    public class MyLogger
    {
        public Component Owner { get; }

        public enum Channel
        {
            One,
            Two,
            Three,
            Four,
            Five,
            Six,

            Count
        }

        private DateTime[] LastStamp { get; } = new DateTime[(int)Channel.Count];

        public MyLogger(Component owner)
        {
            Owner = owner;
            for (int i = 0; i < LastStamp.Length; i++)
                LastStamp[i] = DateTime.MinValue;
        }

        public void WriteLowFrequency(Channel channel, string msg)
        {
            DateTime now = DateTime.Now;
            if (now - LastStamp[(int)channel] < TimeSpan.FromMilliseconds(1000))
                return;
            LastStamp[(int)channel] = now;
            Write(msg);
        }
        public void Write(string msg)
        {
            Log.Write(Log.PathOf(Owner) + $": {msg}");
        }
    }


    public class Echelon : Submersible
    {
        public static GameObject model;
        private EchelonControl control;
        //private RotateCamera rotateCamera;
        private PDA pda;
        private MyLogger EchLog { get; }
        private EchelonEngine engine;

        public Echelon()
        {
            EchLog = new MyLogger(this);
            EchLog.Write($"Constructed");
        }

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

        public override void Awake()
        {
            var existing = GetComponent<VFEngine>();
            if (existing != null)
            {
                EchLog.Write($"Removing existing vfEngine {existing}");
                //HierarchyAnalyzer analyzer = new HierarchyAnalyzer();
                //analyzer.LogToJson(existing, $@"C:\temp\logs\oldEngine.json");
                Destroy(existing);
            }
            VFEngine = Engine = engine = gameObject.AddComponent<EchelonEngine>();
            EchLog.Write($"Assigned new engine");
            base.Awake();
        }

        private void LocalInit()
        {
            if (!isInitialized)
            {
                EchLog.Write($"LocalInit() first time");
                isInitialized = true;
                try
                {
                    //Engine = new VoidEngine();
                    //CanLeviathanGrab = false;
                    //stabilizeRoll = false;
                    //sidewaysTorque = 0;
                    //sidewardForce = 0;
                    //forwardForce = 0;
                    //backwardForce = 0;
                    //verticalForce = 0;
                    //onGroundForceMultiplier = 0;

                    //var dummy = new GameObject("DummyPhysics");
                    //var dummyRb = dummy.AddComponent<Rigidbody>();
                    //dummyRb.isKinematic = true;
                    //dummy.transform.parent = transform;
                    //useRigidbody = dummyRb;

                    //VFEngine = Engine = gameObject.AddComponent<EchelonEngine>();




                    //EchLog.Write($"Start on rb");


                    control = GetComponent<EchelonControl>();
                    //rotateCamera = GetComponentInChildren<RotateCamera>();

                    //if (rotateCamera == null)
                    //    EchLog.Write($"Rotate camera not found");
                    //else
                    //    EchLog.Write($"Found camera rotate {rotateCamera.name}");

                    if (control != null)
                    {
                        EchLog.Write("Found control");
                    }
                    else
                    {
                        if (transform == null)
                            EchLog.Write($"Do not have a transform");
                        else
                        {
                            EchLog.Write($"This is {transform.name}");
                            EchLog.Write("This has components: " + Helper.NamesS(Helper.AllComponents(transform)));
                            EchLog.Write("This has children: " + Helper.NamesS(Helper.Children(transform)));
                        }
                    }
                    EchLog.Write($"LocalInit() done");

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
                EchLog.Write("Echelon.Start()");


                LocalInit();

                base.Start();
                EchLog.Write("Echelon.Start() done");

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
                EchLog.Write("Echelon.PlayerEntry()");
                LocalInit();

                base.PlayerEntry();
                control.Onboard(Player.main.camRoot.transform);

                //playerPosition = Player.main.transform.parent.gameObject;
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
                EchLog.Write("Echelon.PlayerExit()");
                LocalInit();
                control.Offboard();
                base.PlayerExit();

                Player.main.transform.LookAt(transform.position);

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


                Vector2 lookDelta = GameInput.GetLookDelta();
                control.lookRightAxis = lookDelta.x * 0.1f;
                control.lookUpAxis = lookDelta.y * 0.1f;


                //var rb = GetComponent<Rigidbody>();
                //rb.isKinematic = false;
                //rb.mass = 10;
                //rb.angularDrag = 10;
                //rb.drag = 10;
                //rb.useGravity = true;

                //EchLog.WriteLowFrequency(MyLogger.Channel.One,
                //    $"pm={GetPilotingMode()}, ctrl={IsUnderCommand}," +
                //    $" engine.enabled={VFEngine?.enabled}," +
                //    $" pda={Player.main.GetPDA().isOpen}," +
                //    $" av={!AvatarInputHandler.main || AvatarInputHandler.main.IsEnabled()}," +
                //    $" charge={GetComponent<EnergyInterface>().hasCharge}");


                control.outOfWater = !GetIsUnderwater();

                control.cameraCenterIsCockpit = Player.main.pda.state != PDA.State.Closed;
                //var move = GameInput.GetMoveDirection();
                //control.forwardAxis =
                //    GameInput.GetAnalogValueForButton(GameInput.Button.MoveForward)
                //    - GameInput.GetAnalogValueForButton(GameInput.Button.MoveBackward)
                //    ;
                //control.rightAxis =
                //    GameInput.GetAnalogValueForButton(GameInput.Button.MoveRight)
                //    - GameInput.GetAnalogValueForButton(GameInput.Button.MoveLeft)
                //    ;
                //control.upAxis = 
                //    GameInput.GetAnalogValueForButton(GameInput.Button.MoveUp)
                //    - GameInput.GetAnalogValueForButton(GameInput.Button.MoveDown)
                //    ;
                //rotateCamera.rotationAxisX = Input.GetAxis("Mouse X") * 0.1f;
                //rotateCamera.rotationAxisY = Input.GetAxis("Mouse Y") * 0.1f;

                ////rotateCamera.rotationAxisX = 
                ////    GameInput.GetAnalogValueForButton(GameInput.Button.LookRight)
                ////    - GameInput.GetAnalogValueForButton(GameInput.Button.LookLeft);
                ////rotateCamera.rotationAxisY = GameInput.GetAnalogValueForButton(GameInput.Button.LookUp)
                ////    - GameInput.GetAnalogValueForButton(GameInput.Button.LookDown);

                ////if (rotateCamera.rotationAxisX != 0 || rotateCamera.rotationAxisY != 0)
                ////    Log.Write($"Rot: {rotateCamera.rotationAxisX}, {rotateCamera.rotationAxisY}");

                //control.freeCamera =
                //    GameInput.GetAnalogValueForButton(GameInput.Button.RightHand) > 0.5f
                //    ;

                engine.overdriveActive = GameInput.GetAnalogValueForButton(GameInput.Button.Sprint);

                //control.overdriveActive =
                //    GameInput.GetAnalogValueForButton(GameInput.Button.Sprint) > 0.5f
                //    ;

                //if (transform.position.y >= Ocean.GetOceanLevel() - 10)
                //    control.positionCameraBelowSub = true;
                //else if (transform.position.y < Ocean.GetOceanLevel() - 15)
                //    control.positionCameraBelowSub = false;

                //control.isDocked = docked;

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
                var cockpitExit = mainSeat.Find($"ExitLocation");
                if (!mainSeat)
                {
                    EchLog.Write("PilotSeat not found");
                    return default;
                }
                return new VehiclePilotSeat
                {
                    Seat = mainSeat.gameObject,
                    SitLocation = mainSeat.gameObject,
                    ExitLocation = cockpitExit,
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
                    EchLog.Write("Hatch not found");
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
                    EchLog.Write("Clip proxy not found");
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
                    EchLog.Write($"Upgrades interface not found");
                return rs;

            }

        }


        //public override VFEngine VFEngine { get; set; }

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
                EchLog.Write($"Returning {rs.Count} headlight(s)");
                return rs;

            }

        }
    }
}
