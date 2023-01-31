using Moonstorm.Starstorm2;
using RoR2;
using UnityEngine;

namespace EntityStates.Pickups.Extinction
{
    class ExtinctionIdleState : ExtinctionBaseState
    {
        private OverlapAttack attack;
        private float fireStopwatch;
        private float resetStopwatch;

        [Tooltip("The frequency (1/time) at which the overlap attack is tested. Higher values are more accurate but more expensive.")]
        public float fireFrequency = 1f;

        [Tooltip("The frequency (1/time) at which the overlap attack is reset. Higher values means more frequent ticks of damage.")]
        public float resetFrequency = 4f;

        [Tooltip("Should the attacks also damage the owner.")]
        public AttackerFiltering attackerFiltering = AttackerFiltering.AlwaysHit;

        public override void OnEnter()
        {
            base.OnEnter();
            this.ResetOverlap();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                this.resetStopwatch += Time.fixedDeltaTime;
                this.fireStopwatch += Time.fixedDeltaTime;
                if (this.resetStopwatch >= 1f / this.resetFrequency)
                {
                    this.ResetOverlap();
                    this.resetStopwatch -= 1f / this.resetFrequency;
                }
                if (this.fireStopwatch >= 1f / this.fireFrequency)
                {
                    this.attack.Fire(null);
                    this.fireStopwatch -= 1f / this.fireFrequency;
                }
            }
            if (base.isAuthority && (!base.ownerBody || base.ownerBody.inventory.GetItemCount(SS2Content
                .Items.RelicOfExtinction) < 1))
            {
                this.outer.SetNextState(new ExtinctionDieStage());
            }
        }
        private void ResetOverlap()
        {
            this.attack = new OverlapAttack();
            //attack.procChainMask = something goes here!
            attack.procCoefficient = 0; //FUCK YOU
            attack.attacker = base.ownerBody.gameObject;
            attack.inflictor = gameObject; //funballs do this, so we will too
            attack.teamIndex = base.ownerBody.teamComponent.teamIndex;
            attack.attackerFiltering = attackerFiltering;
            attack.damage = base.ownerBody.baseDamage * 2.5f; //Deals 250% of BASE damage
            //this.attack.forceVector = vector + force amount * base.transform.forward;
            //attack.hitEffectPrefab = impactEffect;
            attack.isCrit = false; //FUCK YOU TOO
            attack.damageColorIndex = DamageColorIndex.DeathMark;
            attack.damageType = DamageType.AOE;
            attack.hitBoxGroup = base.GetComponent<HitBoxGroup>();
        }

    }
}
