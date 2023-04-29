using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using Moonstorm.Starstorm2;
using UnityEngine.AddressableAssets;

namespace EntityStates.MULE
{
    public class MULESlam : BaseSkillState
    {
        public float charge;

        public static float minDamageCoefficient;
        public static float maxDamageCoefficient;
        public static float minRadius;
        public static float maxRadius;
        public static float baseDuration;
        public static string muzzleString;

        public static GameObject slamEffectVFX;

        private Animator animator;
        private float duration;
        private float radius;
        private float fireDuration;
        private bool hasFired;
        private float damageCoefficient;
        private ChildLocator childLocator;

        public override void OnEnter()
        {
            Debug.Log("Slamming " + charge);
            base.OnEnter();
            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();
            childLocator = GetModelChildLocator();
            hasFired = false;
            duration = baseDuration / attackSpeedStat;
            characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 0);
            damageCoefficient = Util.Remap(charge, 0f, 1f, minDamageCoefficient, maxDamageCoefficient);
            radius = Util.Remap(charge, 0f, 1f, minRadius, maxRadius);
            fireDuration = 0.25f * duration;

            if (charge >= 0.45f)
            {
                PlayCrossfade("FullBody, Override", "SlamHeavy", "Primary.playbackRate", duration, 0.05f);
            }
            else
            {
                PlayCrossfade("FullBody, Override", "SlamLight", "Primary.playbackRate", duration, 0.05f);
            }
            PlayCrossfade("Gesture, Override", "BufferEmpty", "Primary.playbackRate", duration, 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void GroundSlam()
        {
            bool crit = RollCrit();
            BlastAttack blast = new BlastAttack()
            {
                radius = radius,
                procCoefficient = 1f,
                position = childLocator.FindChild(muzzleString).position,
                attacker = gameObject,
                teamIndex = teamComponent.teamIndex,
                crit = crit,
                baseDamage = damageCoefficient * damageStat,
                damageColorIndex = DamageColorIndex.Default,
                falloffModel = BlastAttack.FalloffModel.None,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = DamageType.Generic,
                impactEffect = EntityStates.Bison.Headbutt.hitEffectPrefab.GetComponent<EffectIndex>()
            };

            blast.Fire();

            EffectManager.SimpleMuzzleFlash(slamEffectVFX, gameObject, muzzleString, true);



            //AddRecoil();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterMotor.velocity = Vector3.zero;

            if (fixedAge >= fireDuration && !hasFired)
            {
                hasFired = true;
                GroundSlam();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
