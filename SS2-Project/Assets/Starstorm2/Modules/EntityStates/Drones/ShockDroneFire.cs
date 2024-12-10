﻿using UnityEngine;
using RoR2;

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
            DamageTypeCombo damageType = DamageType.Shock5s;
            damageType.damageSource = DamageSource.Primary;
            BlastAttack blast = new BlastAttack()
            {
                radius = radius,
                procCoefficient = 1f,
                position = firepos,
                attacker = gameObject,
                teamIndex = teamComponent.teamIndex,
                crit = crit,
                baseDamage = damageCoefficient * damageStat,
                damageColorIndex = DamageColorIndex.Fragile,
                falloffModel = BlastAttack.FalloffModel.None,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = damageType,
            };

            blast.Fire();

            EffectManager.SimpleEffect(blastEffect, firepos, Quaternion.identity, true);
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
