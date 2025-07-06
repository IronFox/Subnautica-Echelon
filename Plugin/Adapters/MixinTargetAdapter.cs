using Subnautica_Echelon.Logs;
using UnityEngine;

namespace Subnautica_Echelon.Adapters
{
    internal class MixinTargetAdapter : TargetAdapter
    {
        private GameObject go;
        private Rigidbody rb;
        private LiveMixin mixin;

        public MixinTargetAdapter(GameObject go, Rigidbody rb, LiveMixin mixin)
            : base(go.GetInstanceID())
        {
            this.go = go;
            this.rb = rb;
            this.mixin = mixin;
            IsCriticalTarget = TargetScanner.IsCriticalTarget(go);
        }



        public override Rigidbody Rigidbody => rb;

        public override GameObject GameObject => go;



        public override float CurrentHealth => mixin.health;

        public override float MaxHealth => mixin.maxHealth;

        public override bool IsAlive => go != null && mixin.IsAlive();
        public override bool IsInvincible => mixin.invincible;

        public override bool IsCriticalTarget { get; }

        public override void DealDamage(Vector3 origin, float damage, GameObject dealer)
        {
            Log.Write($"Dealing {damage} damage to {GameObject.name}");
            mixin.TakeDamage(damage, origin, dealer: dealer);
            //Log.Write($"Health now at {mixin.health}");
        }

        public override bool Equals(object obj)
        {
            return obj is MixinTargetAdapter adapter &&
                   GameObjectInstanceId == adapter.GameObjectInstanceId;
        }

        public override int GetHashCode()
        {
            return 1732276546 + GameObjectInstanceId.GetHashCode();
        }

        public override string ToString() => $"MixinTargetAdapter:{GameObject.name}";
    }
}