using Subnautica_Echelon.Logs;
using UnityEngine;
using VehicleFramework.Admin;
using VehicleFramework.Engines;
using VehicleFramework.Extensions;

namespace Subnautica_Echelon
{
    public class VoidDrive : ModVehicleEngine
    {
        private MyLogger Log { get; }
        public AudioSource? EngineSource1 { get; private set; }
        public AudioSource? EngineSource2 { get; private set; }
        public Echelon? MV { get; private set; }

        public VoidDrive()
        {
            Log = new MyLogger(this);
            //AngularDrag = 10;

        }

        public new void OnDisable()
        {
            EngineSource1?.Stop();
            EngineSource2?.Stop();
            base.OnDisable();
        }

        public float overdriveActive;
        public Vector3 currentInput;
        public bool freeCamera;
        public bool triggerActive;
        public float lastDrainPerSecond;
        public EchelonModule driveUpgrade;
        //public bool insufficientPower;

        public override void Awake()
        {
            WhistleFactor = 1.5f;
            base.Awake();
            MV = GetComponent<Echelon>();
        }
        public override void Start()
        {
            base.Start();
            Sounds = EngineSoundsManager.GetVoice("ShirubaFoxy");
            EngineSource1 = MV!.gameObject.AddComponent<AudioSource>().Register();
            EngineSource1.loop = true;
            EngineSource1.playOnAwake = false;
            EngineSource1.priority = 0;
            EngineSource2 = MV.gameObject.AddComponent<AudioSource>().Register();
            EngineSource2.loop = false;
            EngineSource2.playOnAwake = false;
            EngineSource2.priority = 0;
            EngineSource1.clip = Sounds.hum;
            EngineSource2.clip = Sounds.whistle;
        }

        public override void ControlRotation()
        {
        }



        protected override void PlayEngineHum()
        {
            float value = MainPatcher.PluginConfig.engineSoundVolume / 100f;
            EngineSource1!.volume = EngineHum / 10f * value * HumFactor;
            if (MV!.IsPowered())
            {
                if (!EngineSource1.isPlaying && base.RB.velocity.magnitude > 0.2f)
                {
                    EngineSource1.Play();
                }
            }
            else
            {
                EngineSource1.Stop();
            }
        }

        protected override void PlayEngineWhistle(Vector3 moveDirection)
        {
            if (base.gameObject.GetComponent<Rigidbody>().velocity.magnitude < 1f)
            {
                isReadyToWhistle = true;
            }
            else
            {
                isReadyToWhistle = false;
            }

            if (EngineSource2!.isPlaying)
            {
                if (moveDirection.magnitude == 0f)
                {
                    EngineSource2.Stop();
                }
            }
            else if (isReadyToWhistle && moveDirection.magnitude > 0f)
            {
                float value = MainPatcher.PluginConfig.engineSoundVolume / 100f;
                EngineSource2.volume = value * 0.4f * WhistleFactor;
                EngineSource2.Play();
            }
        }

        //public override void KillMomentum()
        //{
        //    Log.WriteLowFrequency(MyLogger.Channel.Three, ($"KillMomentum()"));
        //    RB.velocity = Vector3.zero;
        //}

        protected override void MoveWithInput(Vector3 moveInput)
        {
            //default = 3/4 * 5 / 2.25 = 166%
            //lateral = 3/4 * 2 / 1.5 = 100%
            var baseSpeed = 2.25f;
            var baseLateral = 1.5f;
            var speedBoost = DriveModule.GetSpeedBoost(driveUpgrade);

            var boost = baseSpeed * speedBoost;
            var lateralBoost = baseLateral * speedBoost;
            //Log.WriteLowFrequency(MyLogger.Channel.Two, $"MoveWithInput({moveInput})");
            currentInput = moveInput;
            moveInput = new Vector3(
                moveInput.x * (baseLateral + lateralBoost * overdriveActive),
                moveInput.y * (baseLateral + lateralBoost * overdriveActive),
                moveInput.z * (baseSpeed + boost * overdriveActive));
            moveInput = GetEffectiveMoveInput(moveInput);
            RB.AddRelativeForce(moveInput, ForceMode.VelocityChange);
        }

        private Vector3 GetEffectiveMoveInput(Vector3 moveInput)
        {
            if (freeCamera)
                return new Vector3(0, 0, moveInput.z);
            return moveInput;
        }

        protected override void DoEngineSounds(Vector3 moveDirection)
        {
            base.DoEngineSounds(GetEffectiveMoveInput(moveDirection));
        }

        protected override void DrainPower(Vector3 moveDirection)
        {

            moveDirection = GetEffectiveMoveInput(moveDirection);
            float energyNeeded = lastDrainPerSecond = M.Sqr(moveDirection) * (
                0.05f
                +
                1f * overdriveActive /** M.Sqr(BoostRelative)*/
                );

            var neededNow = energyNeeded * Time.fixedDeltaTime;
            MV!.energyInterface!.ConsumeEnergy(neededNow);
            //insufficientPower = drained < neededNow * 0.8f;
        }

    }

}