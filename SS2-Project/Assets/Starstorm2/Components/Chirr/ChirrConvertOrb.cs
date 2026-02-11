using RoR2;
using RoR2.Orbs;
using UnityEngine;
namespace SS2.Components
{
    public class ChirrFriendOrb : GenericDamageOrb
    {
        public ChirrFriendTracker tracker;
        public float buffDuration = 8f;
        public float convertHealthFraction = 0.5f;

        public override GameObject GetOrbEffect()
        {
            return UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoDiseaseOrbEffect.prefab").WaitForCompletion();
        }

        public override void OnArrival()
        {
            base.OnArrival();
            if (!this.target) return;

            if(this.tracker && this.tracker.friendOwnership)
            {
                ConvertBehavior convertBehavior = this.target.healthComponent.gameObject.GetComponent<ConvertBehavior>();
                if(!convertBehavior) convertBehavior = this.target.healthComponent.gameObject.AddComponent<ConvertBehavior>();
                convertBehavior.lifetime = this.buffDuration;
                convertBehavior.convertHealthFraction = this.convertHealthFraction;

                convertBehavior.chirrFriendTracker = this.tracker;

                var inflictDotInfo = new InflictDotInfo
                {
                    victimObject = target.healthComponent.gameObject,
                    attackerObject = attacker,
                    dotIndex = Survivors.Chirr.ConvertDotIndex,
                    duration = buffDuration,
                    damageMultiplier = 1.0f,
                };
                DotController.InflictDot(ref inflictDotInfo);
            }

        }

        public class ConvertBehavior : MonoBehaviour, IOnTakeDamageServerReceiver
        {
            public ChirrFriendTracker chirrFriendTracker;
            public CharacterBody body;
            public float convertHealthFraction = 0.5f;
            public float lifetime;
            private float stopwatch;
            private void OnEnable()
            {
                HealthComponent h = base.GetComponent<HealthComponent>();
                this.body = h.body;
                h.AddOnTakeDamageServerReceiver(this);
            }
            private void OnDisable()
            {
                body.healthComponent.RemoveOnTakeDamageServerReceiver(this);
            }
            private void FixedUpdate()
            {
                this.stopwatch += Time.fixedDeltaTime;
                if (this.stopwatch >= lifetime || !body.HasBuff(SS2Content.Buffs.BuffChirrConvert)) Destroy(this);
            }
            public void OnTakeDamageServer(DamageReport damageReport)
            {
                // MIND CONTROLLING PLAYER WOULD BE FUNNY.... but alas...
                if(damageReport.victimBody && !damageReport.victimBody.isPlayerControlled && damageReport.victimBody.healthComponent.combinedHealthFraction < convertHealthFraction)
                {
                    
                    if (chirrFriendTracker && chirrFriendTracker.friendOwnership)
                    {
                        chirrFriendTracker.friendOwnership.AddFriend(damageReport.victimMaster);
                        Destroy(this);
                    }
                }
                
            }
        }

    }
}
