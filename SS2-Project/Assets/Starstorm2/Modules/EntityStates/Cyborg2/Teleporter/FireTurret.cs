using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using RoR2.Orbs;
namespace EntityStates.Cyborg2.Teleporter
{
    public class FireTurret : BaseState
    {
        public static float damageCoefficient = 1f;
        public static float procCoefficient = 0.1f;
        public static float baseDuration = 0.15f;
        public static float bounceRange = 10f;
        public static float range = 128f;
        public static int bounces = 0;
        public CharacterBody owner;
        public CharacterBody activatorBody;

        private float duration;
        private BullseyeSearch search;

        public override void OnEnter()
        {
            base.OnEnter();

            float attackSpeed = this.owner ? this.owner.attackSpeed : 1;
            this.duration = baseDuration / attackSpeed;

            LightningOrb lightningOrb = new LightningOrb();
            lightningOrb.origin = base.transform.position; ///////////// muzzle
            lightningOrb.damageValue = damageCoefficient * this.owner.damage;
            lightningOrb.isCrit = owner.RollCrit();
            lightningOrb.bouncesRemaining = bounces;
            lightningOrb.teamIndex = this.owner.teamComponent.teamIndex;
            lightningOrb.attacker = this.owner.gameObject;
            lightningOrb.procChainMask = default(ProcChainMask);
            lightningOrb.procCoefficient = procCoefficient;
            lightningOrb.lightningType = LightningOrb.LightningType.Loader;
            lightningOrb.damageColorIndex = DamageColorIndex.Default;
            lightningOrb.range = bounceRange;
            lightningOrb.bouncedObjects = new List<HealthComponent>();
            HurtBox hurtBox = this.FindTarget();
            if (hurtBox)
            {
                lightningOrb.target = hurtBox;
                OrbManager.instance.AddOrb(lightningOrb);
            }
        }

        public HurtBox FindTarget()
        {
            if (this.search == null)
            {
                this.search = new BullseyeSearch();
            }

            Vector3 position = base.transform.position;
            Vector3 direction = Vector3.zero;
            BullseyeSearch.SortMode sortMode = BullseyeSearch.SortMode.Distance;
            if (this.activatorBody)
            {
                position = activatorBody.inputBank.aimOrigin;
                direction = activatorBody.inputBank.aimDirection;
                sortMode = BullseyeSearch.SortMode.Angle;
            }

            this.search.searchOrigin = position;
            this.search.searchDirection = direction;
            this.search.teamMaskFilter = TeamMask.allButNeutral;
            this.search.teamMaskFilter.RemoveTeam(this.owner.teamComponent.teamIndex);
            this.search.filterByLoS = true;
            this.search.sortMode = sortMode;
            this.search.maxDistanceFilter = range;
            this.search.RefreshCandidates();
            HurtBox hurtBox = this.search.GetResults().FirstOrDefault();

            return hurtBox;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

    }
}
