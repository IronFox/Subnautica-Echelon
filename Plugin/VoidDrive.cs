using UnityEngine;
using VehicleFramework.Engines;

namespace Subnautica_Echelon
{
    public class VoidDrive : ModVehicleEngine
    {
        private MyLogger Log { get; }
        public VoidDrive()
        {
            Log = new MyLogger(this);
            //AngularDrag = 10;

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
        }
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

        public override void DrainPower(Vector3 moveDirection)
        {

            moveDirection = GetEffectiveMoveInput(moveDirection);
            float energyNeeded = lastDrainPerSecond = M.Sqr(moveDirection) * (
                0.05f
                +
                1f * overdriveActive /** M.Sqr(BoostRelative)*/
                );
            
            var neededNow = energyNeeded * Time.fixedDeltaTime;
            var drained = Mathf.Abs(MV.powerMan.TrySpendEnergy(neededNow));
            //insufficientPower = drained < neededNow * 0.8f;
        }

    }

}