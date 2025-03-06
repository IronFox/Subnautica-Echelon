using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using VehicleFramework;
using VehicleFramework.Assets;
using VehicleFramework.Engines;
using VehicleFramework.VehicleParts;
using VehicleFramework.VehicleTypes;


namespace Subnautica_Echelon
{



    public class Echelon : Submersible, IPowerListener
    {
        public static GameObject model;
        private EchelonControl control;
        //private RotateCamera rotateCamera;
        private MyLogger EchLog { get; }
        private VoidDrive engine;
        private AutoPilot autopilot;
        private EnergyInterface energyInterface;
        private int[] moduleCounts = new int[Enum.GetValues(typeof(EchelonModule)).Length];
        public Echelon()
        {
            EchLog = new MyLogger(this);
            EchLog.Write($"Constructed");
        }

        public static Sprite saveFileSprite, moduleBackground;
        public static Atlas.Sprite craftingSprite, pingSprite;
        public static Atlas.Sprite emptySprite = new Atlas.Sprite(Texture2D.blackTexture);
        public override Atlas.Sprite CraftingSprite => craftingSprite ?? base.CraftingSprite;
        public override Atlas.Sprite PingSprite => pingSprite ?? base.PingSprite;
        public override Sprite SaveFileSprite => saveFileSprite ?? base.SaveFileSprite;
        public override Sprite ModuleBackgroundImage => moduleBackground ?? base.ModuleBackgroundImage;
        public override string Description => "The Echelon is a high-speed sub-aquatic superiority fighter";
        public override string EncyclopediaEntry =>
            "The Echelon is designed with the most demanding environments in mind with extensive hunter seeker capabilities. " +
            "It features automatic integrity and power recovery, and onboard smart torpedo manufacturing. " +
            "Its advanced user interfacing enables holographic sensor reconstruction for increased environmental awareness. " +
            "Automatic targeting and target integrity analysis supports threat classification and prioritization. "+
            "Finally, fired torpedoes intercept locked targets with high efficiency while built-in safety " +
            "mechanics avoid harming the origin craft.\n"+
            "You can craft it at any Mobile Vehicle Bay."
            ;

        public override Dictionary<TechType, int> Recipe =>
            new Dictionary<TechType, int> {
                { TechType.PowerCell, 1 },
                { TechType.Welder, 1 },
                { TechType.AdvancedWiringKit, 2 },
                { TechType.UraniniteCrystal, 5 },
                { TechType.Diamond, 2 },
                { TechType.Kyanite, 2 },
                { TechType.PlasteelIngot, 4 },
            };


        public static void GetAssets()
        {
            try
            {
                Log.Write("Echelon.GetAssets()");
                var modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string bundlePath;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    bundlePath = Path.Combine(modPath, "echelon.osx");
                else
                    bundlePath = Path.Combine(modPath, "echelon");
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
            VFEngine = Engine = engine = gameObject.AddComponent<VoidDrive>();
            EchLog.Write($"Assigned new engine");



            base.Awake();
            var cameraController = gameObject.GetComponentInChildren<VehicleFramework.VehicleComponents.MVCameraController>();
            if (cameraController != null)
            {
                EchLog.Write($"Destroying camera controller {cameraController}");
                Destroy(cameraController );
            }


        }



        private void LocalInit()
        {
            if (!isInitialized)
            {
                EchLog.Write($"LocalInit() first time");
                isInitialized = true;
                try
                {
                    autopilot = GetComponentInChildren<AutoPilot>();

                    if (autopilot != null && MainPatcher.PluginConfig.batteryChargeSpeed > 0)
                    {
                        //"Airon" - weird, partially indecipherable low energy voice
                        //"Chels-E" - high-pitched panicky
                        //"Mikjaw"/"Salli" - just bad
                        //"Turtle" - missing?
                        //autopilot.apVoice.voice = VoiceManager.GetVoice("Salli");
                        autopilot.apVoice.voice = Helper.Clone(autopilot.apVoice.voice);
                        autopilot.apVoice.voice.PowerLow = null;
                        autopilot.apVoice.voice.BatteriesNearlyEmpty = null;
                        autopilot.apVoice.voice.UhOh = null;

                    }

                    energyInterface = GetComponent<EnergyInterface>();
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

        private readonly List<MonoBehaviour> reenableOnExit = new List<MonoBehaviour>();
        public override void PlayerEntry()
        {
            try
            {
                EchLog.Write("Echelon.PlayerEntry()");
                LocalInit();

                base.PlayerEntry();
                control.Onboard(Player.main.camRoot.transform);
                
                reenableOnExit.Clear();
                

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

                foreach (MonoBehaviour behavior in reenableOnExit)
                {
                    EchLog.Write($"Reenabling {behavior.name}");
                    behavior.enabled = true;
                }

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


        private void ProcessEnergyRecharge(out bool lowPower, out bool criticalPower)
        {

            if (energyInterface != null
                && !IngameMenu.main.gameObject.activeSelf)
            {
                float recharge =
                      2.5f  //max 2.5 per second
                    * MainPatcher.PluginConfig.batteryChargeSpeed / 100;

                energyInterface.ModifyCharge(
                    Time.deltaTime
                    * recharge
                    );
                energyInterface.GetValues(out var energyCharge, out var energyCapacity);
                lowPower = energyCharge < energyCapacity * 0.02f;
                criticalPower = energyCharge < energyCapacity * 0.01f;


            }
            else
            {
                lowPower = false;
                criticalPower = false;
            }

        }
        private void ProcessTrigger(bool lowPower)
        {
            if (control.isBoarded && !control.isDocked
                && !control.outOfWater && !lowPower
                && Player.main.pda.state == PDA.State.Closed
                && !IngameMenu.main.gameObject.activeSelf
                )
            {
                control.triggerActive = GameInput.GetAnalogValueForButton(GameInput.Button.LeftHand) > 0.1f;
                control.triggerWasActivated = GameInput.GetButtonDown(GameInput.Button.LeftHand);
                if (control.IsFiring)
                {
                    float drain = 2f;
                    powerMan.TrySpendEnergy(Time.deltaTime * drain);
                }
            }
            else
                control.triggerWasActivated = control.triggerActive = false;

        }

        private void ProcessRegeneration(bool criticalPower)
        {
            control.isHealing = false;
            if (liveMixin != null)
            {
                if (liveMixin.health < liveMixin.maxHealth
                    && liveMixin.IsAlive()
                    && !criticalPower
                    && !IngameMenu.main.gameObject.activeSelf
                    && MainPatcher.PluginConfig.selfHealingSpeed > 0)
                {
                    var healing = liveMixin.maxHealth
                        * Time.deltaTime
                        * 0.02f //max = 2% of max health per second
                        * MainPatcher.PluginConfig.selfHealingSpeed / 100   //default will be 5 seconds per 1%
                        ;

                    var clamped = Mathf.Min(healing, liveMixin.maxHealth - liveMixin.health);
                    var effective = clamped / healing;
                    //Debug.Log($"Healing at delta={Time.deltaTime}");
                    float energyDemand =
                        1 //max 1 energy per second
                        * Time.deltaTime
                        //* MainPatcher.PluginConfig.selfHealingSpeed / 100   //if slower, cost less
                        * effective //if clamped, cost less
                        ;

                    var energyTaken = Mathf.Abs(powerMan.TrySpendEnergy(energyDemand));
                    var actuallyHealed = clamped * energyTaken / energyDemand;
                    liveMixin.AddHealth(actuallyHealed);
                    control.isHealing = true;

                }
                
                    

                control.maxHealth = liveMixin.maxHealth;
                control.currentHealth = liveMixin.health;

            }
        }

        private void ForwardControlAxes()
        {
            if (control.batteryDead || control.powerOff)
            {
                control.forwardAxis = 0;
                control.rightAxis = 0;
                control.upAxis = 0;
            }
            else
            {
                control.forwardAxis = engine.currentInput.z;
                control.rightAxis = engine.currentInput.x;
                control.upAxis = engine.currentInput.y;
            }
        }

        private void ProcessBoost(bool lowPower)
        {

            var boostToggle = !MainPatcher.PluginConfig.holdToBoost;


            if (GameInput.GetButtonDown(GameInput.Button.Sprint) && boostToggle)
            {
                if (control.forwardAxis > 0 && engine.overdriveActive > 0)
                    engine.overdriveActive = 0;
            }

            bool canBoost =
                //!engine.insufficientPower
                //&&
                !lowPower
                ;

            if (boostToggle)
            {
                if (control.forwardAxis <= 0 || !canBoost)
                    engine.overdriveActive = 0;
                else
                    engine.overdriveActive = Mathf.Max(engine.overdriveActive, GameInput.GetAnalogValueForButton(GameInput.Button.Sprint));
            }
            else
                engine.overdriveActive = control.forwardAxis > 0 && canBoost
                    ? GameInput.GetAnalogValueForButton(GameInput.Button.Sprint)
                    : 0;


            control.overdriveActive = engine.overdriveActive > 0.5f;
        }

        /// <summary>
        /// Redetects proximity to the ocean surface and forwards the state to control
        /// </summary>
        private void RepositionCamera()
        {
            if (transform.position.y >= Ocean.GetOceanLevel() - 15 && transform.position.y < 1)
                control.positionCameraBelowSub = true;
            else if (transform.position.y < Ocean.GetOceanLevel() - 20 || transform.position.y > 2)
                control.positionCameraBelowSub = false;
        }

        private bool HasModule(EchelonModule module)
            => moduleCounts[(int)module] > 0;

        private int HighestModule(params EchelonModule[] m)
        {
            for (int i = m.Length-1; i >= 0; i--)
                if (HasModule(m[i]))
                    return i + 1;
            return 0;
        }

        public override void Update()
        {
            try
            {
                LocalInit();

                TrailSpaceTargetText.textDisplay = MainPatcher.PluginConfig.textDisplay;
                EchelonControl.targetArrows = MainPatcher.PluginConfig.targetArrows;

                control.torpedoMark = 
                    HighestModule(EchelonModule.TorpedoMk1, EchelonModule.TorpedoMk2, EchelonModule.TorpedoMk3)
                    ;

                Vector2 lookDelta = GameInput.GetLookDelta();
                control.lookRightAxis = lookDelta.x * 0.1f;
                control.lookUpAxis = lookDelta.y * 0.1f;

                ProcessEnergyRecharge( out var lowPower, out var criticalPower );
                ProcessTrigger(lowPower);
                ProcessRegeneration(criticalPower);
                ForwardControlAxes();

                control.outOfWater = !GetIsUnderwater();
                control.cameraCenterIsCockpit = Player.main.pda.state == PDA.State.Opened;
                control.isDocked = docked;
                TorpedoControl.terrainCollisions = MainPatcher.PluginConfig.torpedoTerrainCollisions;

                if (Player.main.pda.state == PDA.State.Closed && !IngameMenu.main.gameObject.activeSelf)
                {
                    control.zoomAxis = -Input.GetAxis("Mouse ScrollWheel")
                        + 
                        ((Input.GetKey(MainPatcher.PluginConfig.altZoomOut) ? 1f : 0f)
                        - (Input.GetKey(MainPatcher.PluginConfig.altZoomIn) ? 1f : 0f)) * 0.02f
                        ;
                }

                if (GameInput.GetKeyDown(MainPatcher.PluginConfig.toggleFreeCamera))
                    engine.freeCamera = control.freeCamera = !control.freeCamera;

                ProcessBoost(lowPower);
                RepositionCamera();

                if (energyInterface != null)
                {
                    energyInterface.GetValues(out var energyCharge, out var energyCapacity);

                    control.maxEnergy = energyCapacity;
                    control.currentEnergy = energyCharge;
                }

                base.Update();
            }
            catch (Exception ex)
            {
                Log.Write("Echelon.Update()", ex);
            }
        }


        public void OnPowerUp()
        {
            control.powerOff = false;
        }

        public void OnPowerDown()
        {
            control.powerOff = true;
        }

        public void OnBatteryDead()
        {
            control.batteryDead = true;
        }

        public void OnBatteryRevive()
        {
            control.batteryDead = false;
        }

        public void OnBatterySafe()
        {
        }

        public void OnBatteryLow()
        {
        }

        public void OnBatteryNearlyEmpty()
        {
        }

        public void OnBatteryDepleted()
        {
        }

        internal void ChangeCounts(EchelonModule moduleType, bool isAdded)
        {
            if (isAdded)
                moduleCounts[(int)moduleType]++;
            else
            {
                if (moduleCounts[(int)moduleType] == 0)
                {
                    Debug.LogError($"Decrementing {moduleType} to less than zero. Ignoring");
                    return;
                }
                moduleCounts[(int)moduleType]--;
            }
            Debug.Log($"Changed counts of {moduleType} to {moduleCounts[(int)moduleType]}");
        }

        public override int MaxHealth => 2000;
        public override int NumModules => 8;
        public override int BaseCrushDepth => 300;
        public override int CrushDepthUpgrade1 => 200;

        public override int CrushDepthUpgrade2 => 600;

        public override int CrushDepthUpgrade3 => 600;

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

        public override List<VehicleStorage> ModularStorages
        {
            get
            {
                var root = transform.Find("StorageRoot").gameObject;
                var rs = new List<VehicleStorage>();
                if (root == null)
                    return rs;
                for (int i = 0; i < 8; i++)
                {
                    var name = $"Storage{i}";
                    var storageTransform = root.transform.Find(name);
                    if (storageTransform == null)
                    {
                        storageTransform = new GameObject(name).transform;
                        storageTransform.parent = root.transform;
                        storageTransform.localPosition = M.V3(i);
                        Debug.Log($"Creating new storage transform {storageTransform} in {root} @{storageTransform.localPosition} => {storageTransform.position}");
                    }
                    rs.Add(new VehicleStorage
                    {
                        Container = storageTransform.gameObject,
                        Height = 2,
                        Width = 2
                    });
                }
                return rs;

            }
        }
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
                var plugs = transform.Find("Module Plugs");

                var plugProxies = new List<Transform >();
                if (plugs != null)
                {
                    for (int i = 0; i < plugs.childCount; i++)
                    {
                        var plug = plugs.GetChild(i);
                        var position = plug.Find("Module Position");
                        if (position != null)
                            plugProxies.Add(position);
                        else
                            EchLog.Write($"Plug {plug.name} does not have a 'Module Position' child");
                    }
                }
                else
                    EchLog.Write($"Plugs not found");

                EchLog.Write($"Determined {plugProxies.Count} plug(s)");

                if (ui != null)
                {
                    rs.Add(new VehicleUpgrades
                    {
                        Interface = ui.gameObject,
                        Flap = ui.gameObject,
                        ModuleProxies = plugProxies
                    });
                }
                else
                    EchLog.Write($"Upgrades interface not found");
                return rs;

            }

        }

        public override List<VehicleBattery> Batteries
        {
            get
            {
                var rs = new List<VehicleBattery>();


                var batteries = transform.Find("Batteries");

                if (batteries != null)
                {
                    for (int i = 0; i < batteries.childCount; i++)
                    {
                        var b = batteries.GetChild(i);
                        if (b != null)
                        {
                            rs.Add(new VehicleBattery
                            {
                                BatterySlot = b.gameObject,
                                BatteryProxy = b
                            });
                        }
                    }
                }
                else
                    Log.Write($"Unable to locate 'Batteries' child");
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
