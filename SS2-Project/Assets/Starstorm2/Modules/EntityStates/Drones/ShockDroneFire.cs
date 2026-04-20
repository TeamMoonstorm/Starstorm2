using UnityEngine;
using RoR2;
using System.Linq;
using System.Collections.Generic;
using RoR2.Orbs;

namespace EntityStates.ShockDrone
{
    public class ShockDroneFire : BaseSkillState
    {
        public static float duration = 1.0f;
        public static float damageCoefficient = 1.0f;
        public static float radius = 12f;
        public static string muzzleString = "Muzzle";
        public static GameObject blastEffect;
        private ChildLocator childLocator;

        public override void OnEnter()
        {
            base.OnEnter();
            //Animator = GetModelAnimator(); does this thing even need an animator?
            childLocator = GetModelChildLocator();
        }
        public override void OnExit()
        {
            base.OnExit();
            if (isAuthority)
                FireShock();
        }
        public void FireShock()
        {
            Vector3 firepos = childLocator.FindChild(muzzleString).position;
            bool crit = RollCrit();
            DamageTypeCombo damageType = DamageType.Shock5s | DamageTypeCombo.GenericPrimary | DamageTypeExtended.Electrical;

            BullseyeSearch search = new BullseyeSearch();
            search.searchOrigin = firepos;
            search.searchDirection = transform.forward;
            search.sortMode = BullseyeSearch.SortMode.Distance;
            search.teamMaskFilter = TeamMask.all;
            search.teamMaskFilter.RemoveTeam(GetTeam());
            search.filterByLoS = true;
            search.minAngleFilter = 0f;
            search.maxAngleFilter = 360f;
            search.maxDistanceFilter = radius;
            search.RefreshCandidates();

            foreach (HurtBox hurt in search.GetResults())
            {
                SS2.Orbs.ExecutionerTaserOrb teslaOrb = new SS2.Orbs.ExecutionerTaserOrb();
                teslaOrb.bouncedObjects = new List<HealthComponent>();
                teslaOrb.attacker = gameObject;
                teslaOrb.inflictor = gameObject;
                teslaOrb.teamIndex = GetTeam();
                teslaOrb.damageValue = damageCoefficient * damageStat;
                teslaOrb.isCrit = crit;
                teslaOrb.origin = firepos;
                teslaOrb.bouncesRemaining = 0;
                teslaOrb.procCoefficient = 1f;
                teslaOrb.target = hurt;
                teslaOrb.damageColorIndex = DamageColorIndex.Default;
                teslaOrb.damageType = damageType;
                teslaOrb.skinNameToken = "Default";
                OrbManager.instance.AddOrb(teslaOrb);
            }

            // EffectManager.SimpleEffect(blastEffect, firepos, Quaternion.identity, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && duration >= fixedAge)
                outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
