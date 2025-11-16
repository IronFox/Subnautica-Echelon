using Nautilus.Handlers;
using Subnautica_Echelon.Logs;
using Subnautica_Echelon.MaterialAdaptation;
using Subnautica_Echelon.Modules;
using Subnautica_Echelon.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using VehicleFramework.Admin;
using VehicleFramework.AutoPilot;
using VehicleFramework.Engines;
using VehicleFramework.Interfaces;
using VehicleFramework.VehicleBuilding;
using VehicleFramework.VehicleTypes;


namespace Subnautica_Echelon
{



    public class Echelon : Submersible, IPowerListener
    {
        public static GameObject? model;
        private EchelonControl? control;

        public EchelonControl Control => control ?? throw new InvalidOperationException("Echelon Control not initialized");

        public static readonly Color defaultBaseColor = new Color(0xDE, 0xDE, 0xDE) / 255f;
        public static readonly Color defaultStripeColor = new Color(0x3F, 0x4C, 0x7A) / 255f;

        private static int instanceCounter = 0;
        public int InstanceID { get; } = instanceCounter++;

        //tracks true if vehicle death was ever determined. Can't enter in this state
        private bool wasDead;

        private bool destroyed;
        private bool hadUnpausedFrame;

        private float mainSmoothness = GlobalMaterialConfig.DefaultMainSmoothness;
        private float stripeSmoothness = GlobalMaterialConfig.DefaultStripeSmoothness;

        private float deathAge;
        private VoidDrive? engine;
        private AutoPilot? autopilot;
        private AutoPilotVoice? voice;
        internal EnergyInterface? energyInterface;
        private int[] moduleCounts = new int[Enum.GetValues(typeof(EchelonModule)).Length];
        public Echelon()
        {
            PLog.Write($"Echelon {InstanceID} Constructed");
            MaterialFixer = new MaterialFixer(this, LogConfig.Silent);
        }

        private const int ModuleCount = 8;
        //private string[] _slotIDs = GenerateSlotIDs(ModuleCount);

        protected override int UpgradeModuleCount => 8;

        //private static string[] GenerateSlotIDs(int modules)
        //{
        //    var rs = (string[])typeof(ModVehicle).GetMethod("GenerateSlotIDs", BindingFlags.NonPublic | BindingFlags.Static)!
        //        .Invoke(null, [modules, false]);
        //    PLog.Write($"Generated {rs.Length} slot IDs for {modules} modules: {string.Join(", ", rs)}");

        //    return rs;
        //    //string[] array = new string[modules];
        //    //for (int i = 0; i < modules; i++)
        //    //{
        //    //    array[i] = "VehicleFrameworkUpgradeModule" + i;
        //    //}

        //    //return array;
        //}

        //public override string[] slotIDs => _slotIDs;

        //private new Color BaseColor { get; set; }
        //private new Color StripeColor { get; set; }
        public override void OnFinishedLoading()
        {
            base.OnFinishedLoading();
            PLog.Write($"Comparing colors {BaseColor} and {StripeColor}");
            if (BaseColor == Color.white && StripeColor == Color.white)
            {
                PLog.Write($"Resetting white {VehicleName}");
                PaintBaseColor(Vector3.zero, defaultBaseColor);
                PaintStripeColor(Vector3.zero, defaultStripeColor);
            }

        }

        public static Sprite? saveFileSprite, moduleBackground;
        public static Sprite? craftingSprite, pingSprite;
        public static Sprite emptySprite = Sprite.Create(Texture2D.blackTexture, Rect.zero, Vector2.one);
        public override Sprite? CraftingSprite => craftingSprite ?? base.CraftingSprite;
        public override Sprite? PingSprite => pingSprite ?? base.PingSprite;
        public override Sprite? SaveFileSprite => saveFileSprite ?? base.SaveFileSprite;
        public override Sprite? ModuleBackgroundImage => moduleBackground ?? base.ModuleBackgroundImage;
        public override string Description => Language.main.Get("description");
        public override string EncyclopediaEntry => Language.main.Get("encyclopedia");

        public override Dictionary<TechType, int> Recipe =>
            new Dictionary<TechType, int> {
                { TechType.PowerCell, 1 },
                { TechType.AdvancedWiringKit, 1 },
                { TechType.UraniniteCrystal, 2 },
                { TechType.Lead, 3 },
                //{ TechType.Diamond, 1 },
                //{ TechType.Kyanite, 2 },
                { TechType.TitaniumIngot, 2 },
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
                PLog.Exception("Echelon.GetAssets()", ex, null);
            }
        }

        void OnDestroy()
        {
            Log.Write($"{VehicleName} Echelon.OnDestroy()");
            MaterialFixer.Destroy();
            destroyed = true;
        }


        private bool isInitialized = false;

        public override void SubConstructionComplete()
        {
            base.SubConstructionComplete();
            PLog.Write($"SubConstructionComplete: Resetting colors");

            PaintBaseColor(Vector3.zero, defaultBaseColor);
            PaintStripeColor(Vector3.zero, defaultStripeColor);
        }

        public override void Awake()
        {
            try
            {
                var prefabId = GetComponent<PrefabIdentifier>();
                if (prefabId != null)
                    PLog.Write($"Echelon.Awake() '{VehicleName}' <{prefabId.Id}> #{InstanceID} {gameObject.NiceName()}");
                else
                    PLog.Write($"Echelon.Awake() '{VehicleName}' #{InstanceID} {gameObject.NiceName()}");

                var existing = GetComponent<VFEngine>();
                if (existing != null)
                {
                    PLog.Write($"Removing existing vfEngine {existing}");
                    //HierarchyAnalyzer analyzer = new HierarchyAnalyzer();
                    Destroy(existing);
                }
                VFEngine = engine = gameObject.AddComponent<VoidDrive>();
                PLog.Write($"Assigned new engine");

                control = GetComponent<EchelonControl>();
                if (control)
                {
                    PLog.Write("Found Control");
                }
                else
                {
                    PLog.Fail("Did not find Control");
                    if (!transform)
                        PLog.Write($"Do not have a transform");
                    else
                    {
                        PLog.Write($"This is {transform.name}");
                        PLog.Write("This has components: " + Helper.NamesS(Helper.AllComponents(transform)));
                        PLog.Write("This has children: " + Helper.NamesS(transform.GetChildren()));
                    }
                }



                this.onToggle += OnToggleModule;

                MaterialFixer.Awake();

                base.Awake();
                var cameraController = gameObject.GetComponentInChildren<VehicleFramework.VehicleRootComponents.MVCameraController>();
                if (cameraController != null)
                {
                    PLog.Write($"Destroying camera controller {cameraController}");
                    Destroy(cameraController);
                }
            }
            catch (Exception ex)
            {
                PLog.Exception("Echelon.Awake()", ex, gameObject);
            }

        }

        private bool ToggleAnyOn<T>() where T : EchelonModuleFamily<T>
        {
            for (int i = 0; i < slotIDs.Length; i++)
            {
                if (!IsToggled(i))
                {
                    var item = modules.GetItemInSlot(this.slotIDs[i])?.item;
                    if (item && EchelonModuleFamily<T>.IsAny(item!.GetTechType()))
                    {
                        PLog.Write($"Found non-toggled weapon in slot {i}/{slotIDs[i]}. Toggling off");
                        ToggleSlot(i, true);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsAnyToggled<T>() where T : EchelonModuleFamily<T>
        {
            for (int i = 0; i < slotIDs.Length; i++)
            {
                if (IsToggled(i))
                {
                    var item = modules.GetItemInSlot(this.slotIDs[i])?.item;
                    if (item && EchelonModuleFamily<T>.IsAny(item!.GetTechType()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void OnToggleModule(int slotID, bool state)
        {
            PLog.Write($"OnToggleModule({slotID},{state})");
            var item = modules.GetItemInSlot(this.slotIDs[slotID])?.item;
            PLog.Write($"Checking {item}");
            if (IsWeapon(item))
            {
                PLog.Write($"Is weapon");
                if (state)
                {
                    PLog.Write($"Is toggling on");
                    for (int i = 0; i < slotIDs.Length; i++)
                    {
                        if (i != slotID)
                        {
                            var item2 = modules.GetItemInSlot(this.slotIDs[i])?.item;
                            if (IsWeapon(item2) && IsToggled(i))
                            {
                                PLog.Write($"Found other toggled weapon in slot {i}/{slotIDs[i]}. Toggling off");
                                ToggleSlot(i, false);
                            }
                        }
                    }
                }
            }
        }

        private bool IsWeapon(Pickupable? item)
        {
            if (!item)
                return false;
            var tt = item!.GetTechType();
            return TorpedoModule.IsAny(tt) || RailgunModule.IsAny(tt);
        }

        public override bool AutoApplyShaders => false;

        private void LocalInit()
        {
            if (!isInitialized)
            {
                PLog.Write($"LocalInit() first time");
                isInitialized = true;
                try
                {
                    autopilot = GetComponentInChildren<AutoPilot>();
                    voice = GetComponentInChildren<AutoPilotVoice>();

                    if (autopilot != null/* && MainPatcher.PluginConfig.batteryChargeSpeed > 0*/)
                    {
                        //"Airon" - weird, partially indecipherable low energy voice
                        //"Chels-E" - high-pitched panicky
                        //"Mikjaw"/"Salli" - just bad
                        //"Turtle" - missing?
                        //autopilot.apVoice.voice = VoiceManager.GetVoice("ShirubaFoxy");
                        voice.voice = Helper.Clone(VoiceManager.GetVoice("ShirubaFoxy"));
                        voice.voice.PowerLow = null;
                        voice.voice.BatteriesNearlyEmpty = null;
                        voice.voice.UhOh = null;

                    }

                    energyInterface = GetComponent<EnergyInterface>();
                    PLog.Write($"LocalInit() done");

                }
                catch (Exception e)
                {
                    PLog.Exception("LocalInit()", e, gameObject);
                }

            }
        }

        private bool recursingColor = false;
        private bool IsBlack(Color col)
        {
            return col.r == 0 && col.g == 0 && col.b == 0;
        }

        private void AllocateColors()
        {
            if (vehicleColors is null || vehicleColors.Length < 5)
                vehicleColors = new Vector3[5];
        }

        protected override void PaintBaseColor(Vector3 hsb, Color color)
        {
            if (recursingColor)
            {
                PLog.Write("Recursing color, returning");
                return;
            }
            PLog.Write($"Updating sub base color to {color}");
            try
            {
                if (!IsBlack(color))
                    nonBlackBaseColor = color;
                else
                    color = nonBlackBaseColor;

                //base.BaseColor = color;

                recursingColor = true;
                base.PaintBaseColor(hsb, color);
                base.SetBaseColor(color);
                AllocateColors();
                vehicleColors[0] = new Vector3(color.r, color.g, color.b);

                if (subName)
                {
                    subName.SetColor(0, hsb, color);
                }
                recursingColor = false;

            }
            catch (Exception ex)
            {
                PLog.Exception($"Forwarding to base.{nameof(SetBaseColor)}({hsb},{color})", ex, gameObject);
                recursingColor = false;
            }
            UpdateColors(false);
        }


        protected override void PaintStripeColor(Vector3 hsb, Color color)
        {
            if (recursingColor)
            {
                PLog.Write($"Recursing color, returning with {hsb} {color}");
                return;
            }
            PLog.Write($"Updating sub stripe color to {color}");
            try
            {
                if (!IsBlack(color))
                    nonBlackStripeColor = color;
                else
                    color = nonBlackStripeColor;
                //StripeColor = color;

                recursingColor = true;
                base.PaintStripeColor(hsb, color);
                base.SetStripeColor(color);
                AllocateColors();
                vehicleColors[3] = new Vector3(color.r, color.g, color.b);
                if (subName)
                {
                    subName.SetColor(3, hsb, color);
                }
                recursingColor = false;
            }
            catch (Exception ex)
            {
                PLog.Exception($"Forwarding to base.{nameof(SetStripeColor)}({hsb},{color})", ex, gameObject);
                recursingColor = false;
            }
            UpdateColors(false);
        }


        private void UpdateColors(bool force)
        {
            try
            {
                var listeners = GetComponentsInChildren<IColorListener>();
                foreach (var listener in listeners)
                {
                    try
                    {
                        listener.SetColors(BaseColor, mainSmoothness, StripeColor, stripeSmoothness, force);
                    }
                    catch (Exception ex)
                    {
                        PLog.Exception($"Forwarding to {listener}.{nameof(listener.SetColors)}({BaseColor},{StripeColor})", ex, gameObject);
                    }
                }
            }
            catch (Exception ex)
            {
                PLog.Exception($"Getting listeners", ex, gameObject);
            }
        }


        public override void Start()
        {
            try
            {
                PLog.Write("Echelon.Start()");


                LocalInit();

                base.Start();
                PLog.Write("Echelon.Start() done");

            }
            catch (Exception ex)
            {
                PLog.Exception("Echelon.Start()", ex, gameObject);
            }
        }

        private readonly List<MonoBehaviour> reenableOnExit = new List<MonoBehaviour>();
        public override void PlayerEntry()
        {
            try
            {
                if (!liveMixin.IsAlive() || wasDead)
                {
                    ErrorMessage.AddError(string.Format(Language.main.Get("destroyedAndCannotBeBoarded"), VehicleName));
                    return;
                }

                PLog.Write("Echelon.PlayerEntry()");
                LocalInit();

                CraftDataHandler.SetQuickSlotType(TechType.VehicleStorageModule, QuickSlotType.Instant);


                base.PlayerEntry();
                Control.Onboard(Player.main.camRoot.transform);

                reenableOnExit.Clear();

                DoSanityCheck();

                //playerPosition = Player.main.transform.parent.gameObject;
            }
            catch (Exception ex)
            {
                PLog.Exception("Echelon.PlayerEntry()", ex, gameObject);
            }
        }

        public override void PlayerExit()
        {
            try
            {
                if (!IsUnderCommand)
                    return; //there appears to be some double execution
                PLog.Write("Echelon.PlayerExit()");
                LocalInit();
                Control.Offboard();
                base.PlayerExit();

                foreach (MonoBehaviour behavior in reenableOnExit)
                {
                    PLog.Write($"Reenabling {behavior.name}");
                    behavior.enabled = true;
                }
                CraftDataHandler.SetQuickSlotType(TechType.VehicleStorageModule, QuickSlotType.None);

                Player.main.transform.LookAt(transform.position);
                DoSanityCheck();

            }
            catch (Exception ex)
            {
                PLog.Exception("Echelon.PlayerExit()", ex, gameObject);
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
                    PLog.Exception("Echelon.FixedUpdate()", ex, gameObject);
                }
            }
        }

        private EchelonModule GetTorpedoMark() => TorpedoModule.GetFrom(this);
        private EchelonModule GetRailgunMark() => RailgunModule.GetFrom(this);
        private EchelonModule GetBatteryMark() => NuclearBatteryModule.GetFrom(this);
        private EchelonModule GetDriveMark() => DriveModule.GetFrom(this);
        private EchelonModule GetSelfRepairMark() => RepairModule.GetFrom(this);

        private void ProcessEnergyRecharge(out bool lowPower, out bool criticalPower)
        {

            if (energyInterface != null
                && !IngameMenu.main.gameObject.activeSelf)
            {
                var batteryMk = GetBatteryMark();

                float level = Mathf.Pow(2, NuclearBatteryModule.LevelOf(batteryMk) + 1);

                float recharge =
                      0.4f  //max 1.6 per second
                    * level;

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

        public const float RailgunEnergyCost = 12;
        public bool CanControl
            => Control.isBoarded
            && !Control.isDocked
            && Player.main.pda.state == PDA.State.Closed
            && !IngameMenu.main.gameObject.activeSelf;

        private void ProcessTrigger(bool lowPower)
        {
            if (CanControl
                && !Control.outOfWater && !lowPower
                )
            {
                Control.triggerActive = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
                Control.triggerWasActivated = GameInput.GetButtonDown(GameInput.Button.LeftHand);
                if (Control.IsFiring)
                {

                    float drain = 2f;

                    if (Control.activeWeapon == Weapon.Railgun)
                    {

                        if (Control.RailgunIsCharging)
                            drain = RailgunEnergyCost;
                    }

                    energyInterface!.ConsumeEnergy(Time.deltaTime * drain);
                }
            }
            else
                Control.triggerWasActivated = Control.triggerActive = false;

            if (Control.RailgunIsDischarging)
            {
                //PLog.Write($"Discharging");
                energyInterface!.AddEnergy(RailgunEnergyCost * Time.deltaTime * RailgunCharge.DischargeSpeedFactor);
            }


        }

        private void ProcessRegeneration(bool criticalPower)
        {
            Control.isHealing = false;

            var delta = Time.deltaTime;

            if (liveMixin != null)
            {
                var level = RepairModule.GetRelativeSelfRepair(RepairModule.GetFrom(this));

                if (liveMixin.health < liveMixin.maxHealth
                    && liveMixin.IsAlive()
                    && !criticalPower
                    && !IngameMenu.main.gameObject.activeSelf
                    //&& MainPatcher.PluginConfig.selfHealingSpeed > 0
                    && delta > 0
                    && level > 0)
                {
                    var healing = liveMixin.maxHealth
                        * delta
                        * level
                        //* 0.02f //max = 2% of max health per second
                        //* MainPatcher.PluginConfig.selfHealingSpeed / 100   //default will be 5 seconds per 1%
                        ;

                    var clamped = Mathf.Min(healing, liveMixin.maxHealth - liveMixin.health);
                    var effective = clamped / healing;
                    float energyDemand =
                        1 //max 1 energy per second
                        * delta
                        //* MainPatcher.PluginConfig.selfHealingSpeed / 100   //if slower, cost less
                        * effective //if clamped, cost less
                        ;

                    energyInterface!.ConsumeEnergy(energyDemand);


                    var actuallyHealed = clamped;
                    liveMixin.AddHealth(actuallyHealed);
                    Control.isHealing = true;

                }


                Control.maxHealth = liveMixin.maxHealth;
                Control.currentHealth = liveMixin.health;

            }
        }

        private void ForwardControlAxes()
        {
            if (Control.batteryDead || Control.powerOff)
            {
                Control.forwardAxis = 0;
                Control.rightAxis = 0;
                Control.upAxis = 0;
            }
            else
            {
                Control.forwardAxis = engine!.currentInput.z;
                Control.rightAxis = engine.currentInput.x;
                Control.upAxis = engine.currentInput.y;
            }
        }

        private void ProcessBoost(bool lowPower)
        {

            var boostToggle = !MainPatcher.PluginConfig.holdToBoost;

            engine!.driveUpgrade = HighestModuleType(EchelonModule.DriveMk1, EchelonModule.DriveMk2, EchelonModule.DriveMk3);

            if (GameInput.GetButtonDown(GameInput.Button.Sprint) && boostToggle)
            {
                if (Control.forwardAxis > 0 && engine.overdriveActive > 0)
                    engine.overdriveActive = 0;
            }

            bool canBoost =
                //!engine.insufficientPower
                //&&
                !lowPower
                //&& engine.driveUpgrade != EchelonModule.None
                ;

            if (boostToggle)
            {
                if (Control.forwardAxis <= 0 || !canBoost)
                    engine.overdriveActive = 0;
                else
                    engine.overdriveActive = Mathf.Max(engine.overdriveActive, GameInput.GetButtonHeld(GameInput.Button.Sprint) ? 1 : 0);
            }
            else
                engine.overdriveActive = Control.forwardAxis > 0 && canBoost
                    ? GameInput.GetButtonHeld(GameInput.Button.Sprint) ? 1 : 0
                    : 0;


            Control.overdriveActive = engine.overdriveActive > 0.5f;
            Control.overdriveIntensity = DriveModule.GetSpeedBoost(engine.driveUpgrade);
        }

        /// <summary>
        /// Redetects proximity to the ocean surface and forwards the state to Control
        /// </summary>
        private void RepositionCamera()
        {
            if (transform.position.y >= Ocean.GetOceanLevel() - 15 && transform.position.y < 1)
                Control.positionCameraBelowSub = true;
            else if (transform.position.y < Ocean.GetOceanLevel() - 20 || transform.position.y > 2)
                Control.positionCameraBelowSub = false;
        }

        private bool HasModule(EchelonModule module)
            => moduleCounts[(int)module] > 0;

        public EchelonModule HighestModuleType(params EchelonModule[] m)
        {
            for (int i = m.Length - 1; i >= 0; i--)
                if (HasModule(m[i]))
                    return m[i];
            return EchelonModule.None;
        }

        public override void OnUpgradeModuleUse(TechType techType, int slotID)
        {
            try
            {
                PLog.Write($"OnUpgradeModuleUse({techType}, {slotID})");
                base.OnUpgradeModuleUse(techType, slotID);
                if (techType == TechType.VehicleStorageModule)
                {
                    var storage = GetStorageInSlot(slotID, techType);
                    if (storage != null)
                    {
                        PLog.Write($"Valid container. Opening...");
                        PDA pda = Player.main.GetPDA();
                        Inventory.main.SetUsedStorage(storage);
                        pda.Open(PDATab.Inventory);
                    }
                }
            }
            catch (Exception ex)
            {
                PLog.Exception("OnUpgradeModuleUse()", ex, gameObject);
            }
        }

        public override void OnVehicleUndocked()
        {
            PLog.Write($"[{InstanceID}] Echelon.OnVehicleUndocked() isScuttled: " + this.IsScuttled);
            base.OnVehicleUndocked();
            MaterialFixer.OnVehicleUndocked();

            DoSanityCheck();


        }


        private void DoSanityCheck()
        {

            PLog.Write($"[{InstanceID}] Performing sanity check");
            if (!IsScuttled)
            {
                if (!docked)
                {
                    var colliders = GetComponentsInChildren<Collider>();
                    PLog.Write($"[{InstanceID}] Found {colliders.Length} colliders in children");
                    MeshCollider? foundMesh = null;
                    foreach (var collider in colliders)
                    {
                        if (collider.transform.IsChildOf(Player.main.transform))
                        {
                            PLog.Write($"[{InstanceID}] Skipping collider {collider.NiceName()} because it is a child of player");
                            continue;
                        }
                        if (collider is BoxCollider box)
                        {
                            if (!collider.enabled)
                                continue;

                            if (collider.transform.GetComponentInParent<VehicleFramework.VehicleChildComponents.VehicleHatch>() != null)
                            {
                                PLog.Write($"[{InstanceID}] Skipping box collider {box.NiceName()} because it is a child of a hatch");
                                continue;
                            }
                            if (collider.transform.IsChildOf(PilotSeat.Seat.transform))
                            {
                                PLog.Write($"[{InstanceID}] Skipping box collider {box.NiceName()} because it is a child of pilot seat");
                                continue;
                            }
                            if (Upgrades.Any(x => collider.transform.IsChildOf(x.Interface.transform) || collider.transform.IsChildOf(x.Flap!.transform)))
                            {
                                PLog.Write($"[{InstanceID}] Skipping box collider {box.NiceName()} because it is an upgrade interface");
                                continue;
                            }
                            if (Batteries.Any(x => collider.transform.IsChildOf(x.BatterySlot.transform)))
                            {
                                PLog.Write($"[{InstanceID}] Skipping box collider {box.NiceName()} because it is a battery interface");
                                continue;
                            }
                            if (collider.enabled)
                            {
                                PLog.Write($"[{InstanceID}] Found enabled box collider {box.NiceName()} with size {box.size}. Disabling");
                                collider.enabled = false;
                            }
                        }
                        else if (collider is MeshCollider mesh)
                        {
                            if (foundMesh != null)
                            {
                                PLog.Fail($"[{InstanceID}] Found multiple mesh colliders in {VehicleName}. This is not allowed. Disabling {mesh.NiceName()}");
                                collider.enabled = false;
                                continue;
                            }
                            if (!collider.enabled)
                            {
                                PLog.Write($"[{InstanceID}] Found disabled mesh collider {mesh.NiceName()}. Disabling");
                                collider.enabled = true;
                                foundMesh = mesh;
                            }
                        }
                        else
                        {
                            PLog.Fail($"[{InstanceID}] Found unsupported collider type {collider.NiceName()} in {VehicleName}. Disabling");
                            collider.enabled = false;
                        }
                    }




                    foreach (var h in this.Hatches)
                    {
                        var go = h.Hatch;
                        var hatch = go.GetComponent<VehicleFramework.VehicleChildComponents.VehicleHatch>();
                        if (hatch == null)
                        {
                            PLog.Fail($"[{InstanceID}] Found hatch without VehicleHatch component in {VehicleName}. Creating");
                            hatch = go.EnsureComponent<VehicleFramework.VehicleChildComponents.VehicleHatch>();
                            hatch.mv = this;
                            hatch.EntryLocation = h.EntryLocation;
                            hatch.ExitLocation = h.ExitLocation;
                            hatch.SurfaceExitLocation = h.SurfaceExitLocation;
                            ((IDockListener)hatch).OnUndock();
                        }

                        var box = go.GetComponent<BoxCollider>();
                        if (box == null)
                        {
                            PLog.Fail($"[{InstanceID}] Hatch {h.Hatch.NiceName()} does not have a BoxCollider. Creating");
                            box = go.EnsureComponent<BoxCollider>();
                            box.size = new Vector3(1, 1, 1);
                            box.center = Vector3.zero;
                        }
                        else
                        {
                            if (!box.enabled)
                            {
                                PLog.Fail($"[{InstanceID}] Hatch {h.Hatch.NiceName()} has disabled BoxCollider. Enabling");
                                box.enabled = true;
                            }
                        }

                        if (hatch.mv != this)
                        {
                            PLog.Fail($"[{InstanceID}] Hatch {h.Hatch.NiceName()} is attached to {hatch.mv.NiceName()}, not this vehicle. Fixing...");
                            hatch.mv = this;
                        }

                        if (Drone.MountedDrone)
                        {
                            PLog.Fail($"[{InstanceID}] Hatch {h.Hatch.NiceName()} is not a valid hand target because drone is mounted. Resetting...");
                            typeof(Drone).GetProperty(nameof(Drone.MountedDrone), BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
                            //Drone.StopControlling();
                        }

                        if (!hatch.isValidHandTarget)
                        {
                            PLog.Fail($"[{InstanceID}] Hatch {h.Hatch.NiceName()} is not a valid hand target. Resetting...");
                            hatch.isValidHandTarget = true;
                        }

                        if (!hatch.isActiveAndEnabled)
                        {
                            PLog.Fail($"[{InstanceID}] Hatch {h.Hatch.NiceName()} is not active. Enabling...");
                            hatch.enabled = true;
                            var current = go;
                            while (go != null && go != gameObject)
                            {
                                if (!go.activeSelf)
                                {
                                    PLog.Fail($"[{InstanceID}] Hatch {h.Hatch.NiceName()} is not active in hierarchy. Activating parent {current.name}");
                                    go.SetActive(true);
                                }
                                var p = go.transform.parent;
                                go = p ? p.gameObject : null;
                            }
                        }
                        var live = hatch.GetType().GetField("isLive", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (live != null)
                        {
                            var value = live.GetValue(hatch);
                            if (!(value is bool b))
                                PLog.Fail($"[{InstanceID}] Hatch {h.Hatch.NiceName()} isLive: {value} (not a bool)");
                            else
                            if (!b)
                            {
                                PLog.Fail($"[{InstanceID}] Hatch {h.Hatch.NiceName()} is not live");
                                ((IDockListener)hatch).OnUndock();
                            }
                        }
                        else
                            PLog.Fail($"[{InstanceID}] Hatch {h.Hatch.NiceName()} does not have isLive field");
                    }

                }
                else
                    PLog.Write($"[{InstanceID}] Vehicle is docked, skipping collider sanity check");

            }
            else
                PLog.Write($"[{InstanceID}] Vehicle is scuttled, skipping sanity check");
        }


        private MaterialFixer MaterialFixer;

        private Color nonBlackBaseColor;
        private Color nonBlackStripeColor;

        public override void OnVehicleDocked(Vector3 exitLocation)
        {
            base.OnVehicleDocked(exitLocation);
            MaterialFixer.OnVehicleDocked();
            //SetBaseColor(Vector3.zero, nonBlackBaseColor);
            //SetStripeColor(Vector3.zero, nonBlackStripeColor);
        }

        public override void Update()
        {
            try
            {
                hadUnpausedFrame |= Time.deltaTime > 0;

                LocalInit();

                {
                    float mainShininess;
                    float stripeShininess;
                    if (MainPatcher.PluginConfig.overrideShininess)
                    {
                        mainShininess = MainPatcher.PluginConfig.mainShininessLevel / 100f;
                        stripeShininess = MainPatcher.PluginConfig.stripeShininessLevel / 100f;
                    }
                    else
                    {
                        mainShininess = GlobalMaterialConfig.DefaultMainSmoothness;
                        stripeShininess = GlobalMaterialConfig.DefaultStripeSmoothness;
                    }
                    if (mainSmoothness != mainShininess || stripeSmoothness != stripeShininess)
                    {
                        PLog.Write($"Overriding shininess to main={mainShininess} stripe={stripeShininess}");
                        mainSmoothness = mainShininess;
                        stripeSmoothness = stripeShininess;
                        UpdateColors(true);
                    }
                }

                //MaterialFixer.UniformShininess = MainPatcher.PluginConfig.uniformShininess
                //        ? MainPatcher.PluginConfig.uniformShininessLevel / 100f
                //        : (float?)null;

                if (MaterialFixer.OnUpdate())
                {
                    UpdateColors(true);

                }

                if (Input.GetKeyDown(KeyCode.F6))
                {
                    //if (Player.main.currentMountedVehicle != null)
                    //{
                    //    HierarchyAnalyzer a = new HierarchyAnalyzer();
                    //    a.LogToJson(Player.main.currentMountedVehicle.transform, $@"C:\temp\vehicle.json");
                    //}

                    PLog.Write($"Reapplying materials");
                    MaterialFixer.ReApply();
                }




                if (!liveMixin.IsAlive() || wasDead)
                {
                    wasDead = true;
                    deathAge += Time.deltaTime;
                    if (deathAge > 1.5f)
                    {
                        PLog.Write($"Emitting pseudo self destruct");
                        Control.SelfDestruct(true);
                        PLog.Write($"Calling OnSalvage");
                        OnSalvage();
                        enabled = false;
                        PLog.Write($"Done?");
                        return;
                    }
                }

                TrailSpaceTargetText.textDisplay = MainPatcher.PluginConfig.textDisplay;
                EchelonControl.targetArrows = MainPatcher.PluginConfig.targetArrows;
                EchelonControl.markerDisplay = MainPatcher.PluginConfig.targetHealthMarkers;

                Control.targetMarkerSizeScale = MainPatcher.PluginConfig.targetMarkerSizeScale / 100f;
                RailgunTriggerGuidance.Mk1Damage = MainPatcher.PluginConfig.mk1RailgunDamage;
                RailgunTriggerGuidance.Mk2Damage = MainPatcher.PluginConfig.mk2RailgunDamage;
                Railgun.SoundLevel = MainPatcher.PluginConfig.railgunSoundLevel / 100f;
                CameraShake.GlobalScale = MainPatcher.PluginConfig.globalCameraShakeIntensity / 100f;
                CameraShake.BoostScale = MainPatcher.PluginConfig.boostCameraShakeIntensity / 100f;


                Control.torpedoMark = TorpedoModule.LevelOf(GetTorpedoMark()) + 1;
                Control.railgunMark = RailgunModule.LevelOf(GetRailgunMark()) + 1;
                if (Control.torpedoMark > 0 || Control.railgunMark > 0)
                {
                    if (IsAnyToggled<TorpedoModule>())
                        Control.activeWeapon = Weapon.Torpedoes;
                    else if (IsAnyToggled<RailgunModule>())
                        Control.activeWeapon = Weapon.Railgun;
                    else if (ToggleAnyOn<RailgunModule>())
                        Control.activeWeapon = Weapon.Railgun;
                    else
                    {
                        ToggleAnyOn<TorpedoModule>();
                        Control.activeWeapon = Weapon.Torpedoes;
                    }
                }

                Vector2 lookDelta = GameInput.GetLookDelta();
                Control.lookRightAxis = lookDelta.x * 1e-3f * MainPatcher.PluginConfig.lookSensitivity;
                Control.lookUpAxis = lookDelta.y * 1e-3f * MainPatcher.PluginConfig.lookSensitivity;

                ProcessEnergyRecharge(out var lowPower, out var criticalPower);
                ProcessTrigger(lowPower);
                ProcessRegeneration(criticalPower);
                ForwardControlAxes();

                Control.outOfWater = !GetIsUnderwater();
                Control.cameraCenterIsCockpit = Player.main.pda.state == PDA.State.Opened;
                Control.isDocked = docked;
                TorpedoControl.terrainCollisions = MainPatcher.PluginConfig.torpedoTerrainCollisions;

                if (Player.main.pda.state == PDA.State.Closed && !IngameMenu.main.gameObject.activeSelf)
                {
                    Control.zoomAxis = -Input.GetAxis("Mouse ScrollWheel")
                        +
                        ((GameInput.GetButtonHeld(MainPatcher.PluginConfig.altZoomOut) ? 1f : 0f)
                        - (GameInput.GetButtonHeld(MainPatcher.PluginConfig.altZoomIn) ? 1f : 0f)) * Time.deltaTime
                        ;
                }

                if (CanControl && GameInput.GetButtonDown(MainPatcher.PluginConfig.toggleFreeCamera))
                    engine!.freeCamera = Control.freeCamera = !Control.freeCamera;

                ProcessBoost(lowPower);
                RepositionCamera();

                if (energyInterface != null)
                {
                    energyInterface.GetValues(out var energyCharge, out var energyCapacity);

                    Control.maxEnergy = energyCapacity;
                    Control.currentEnergy = energyCharge;
                }

                base.Update();
            }
            catch (Exception ex)
            {
                PLog.Exception("Echelon.Update()", ex, gameObject);
            }
        }


        public void OnPowerUp()
        {
            Control.powerOff = false;
        }

        public void OnPowerDown()
        {
            Control.powerOff = true;
        }

        public void OnBatteryDead()
        {
            Control.batteryDead = true;
        }

        public void OnBatteryRevive()
        {
            Control.batteryDead = false;
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

        internal void SetModuleCount(EchelonModule moduleType, int count)
        {
            var tm = GetTorpedoMark();
            var bm = GetBatteryMark();
            var dm = GetDriveMark();
            var rm = GetSelfRepairMark();
            var gm = GetRailgunMark();
            moduleCounts[(int)moduleType] = count;
            var tm2 = GetTorpedoMark();
            var bm2 = GetBatteryMark();
            var dm2 = GetDriveMark();
            var rm2 = GetSelfRepairMark();
            var gm2 = GetRailgunMark();
            if (!destroyed && hadUnpausedFrame)
            {
                if (tm != tm2)
                    ErrorMessage.AddMessage(string.Format(Language.main.Get($"torpedoCapChanged"), VehicleName, Language.main.Get("cap_t_" + tm2)));
                if (bm != bm2)
                    ErrorMessage.AddMessage(string.Format(Language.main.Get($"batteryCapChanged"), VehicleName, Language.main.Get("cap_b_" + bm2)));
                if (dm != dm2)
                    ErrorMessage.AddMessage(string.Format(Language.main.Get($"boostCapChanged"), VehicleName, Language.main.Get("cap_d_" + dm2)));
                if (rm != rm2)
                    ErrorMessage.AddMessage(string.Format(Language.main.Get($"repairCapChanged"), VehicleName, Language.main.Get("cap_r_" + rm2)));
                if (gm != gm2)
                    ErrorMessage.AddMessage(string.Format(Language.main.Get($"railgunCapChanged"), VehicleName, Language.main.Get("cap_g_" + gm2)));
            }
            PLog.Write($"Changed counts of {moduleType} to {moduleCounts[(int)moduleType]}");
        }

        public string VehicleName => subName != null ? subName.GetName() : vehicleName;

        public override int MaxHealth => 2000;
        public override int NumModules => ModuleCount;
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
                    PLog.Write("PilotSeat not found");
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
                    PLog.Write("Hatch not found");
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

        public override GameObject VehicleModel => model!;

        public override GameObject[] CollisionModel => new GameObject[] { transform.Find("CollisionModel").gameObject };
        public override BoxCollider BoundingBoxCollider => transform.Find("BoundingBox").gameObject.GetComponent<BoxCollider>();
        public override PilotingStyleEnum PilotingStyle => PilotingStyleEnum.Other;

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
                    if (!storageTransform)
                    {
                        storageTransform = new GameObject(name).transform;
                        storageTransform.parent = root.transform;
                        storageTransform.localPosition = M.V3(i) * 0.01f;
                        storageTransform.localScale = Vector3.one * 0.01f;
                        PLog.Write($"Creating new storage transform {storageTransform} in {root} @{storageTransform.localPosition} => {storageTransform.position}");
                    }
                    rs.Add(new VehicleStorage
                    {
                        Container = storageTransform.gameObject,
                        Height = 10,
                        Width = 8
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
                    PLog.Write("Clip proxy not found");
                return rs;
            }
        }

        private List<VehicleUpgrades>? upgrades = null;
        public override List<VehicleUpgrades> Upgrades
        {
            get
            {
                if (upgrades != null)
                    return upgrades;
                var rs = new List<VehicleUpgrades>();
                var ui = transform.Find("UpgradesInterface");
                var plugs = transform.Find("Module Plugs");

                var plugProxies = new List<Transform>();
                if (plugs != null)
                {
                    for (int i = 0; i < plugs.childCount; i++)
                    {
                        var plug = plugs.GetChild(i);
                        var position = plug.Find("Module Position");
                        if (position != null)
                            plugProxies.Add(position);
                        else
                            PLog.Write($"Plug {plug.name} does not have a 'Module Position' child");
                    }
                }
                else
                    PLog.Write($"Plugs not found");

                PLog.Write($"Determined {plugProxies.Count} plug(s)");

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
                    PLog.Write($"Upgrades interface not found");
                return upgrades = rs;

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
                    PLog.Exception("HeadLights", ex, gameObject);
                }
                PLog.Write($"Returning {rs.Count} headlight(s)");
                return rs;

            }

        }

        public void OnModuleRemoved(int slot, EchelonModule module)
        {
            ToggleSlot(slot, false);    //just in case
        }
    }
}
