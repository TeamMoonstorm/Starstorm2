using HG;
using R2API;
using RoR2;
using SS2.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.MulT
{
    internal class Blowtorch : BaseSkillState
    {
        public float baseDuration = 0.5f;
        private float duration;

        public static float baseTickFrequency;
        public static float baseEntryDuration;
        public static float pressureDuration;

        private float stopwatch;
        private float flamethrowerStopwatch;
        private float entryDuration;

        private bool hasBegunFlamethrower;

        public static float maxDistance;
        public static float heatPerTick;
        public static float tickProcCoefficient;
        public static float tickDamageCoefficient;
        public static float igniteChanceHighHeat;
        public static float heatIgniteThreshold;
        public static float recoilForce;
        public static float force;
        public static float radius;
        public static string muzzleString;

        public static GameObject impactEffectPrefab;
        public static GameObject flameEffectPrefab;

        private void Fire(string muzzleString)
        {
            characterBody.SetAimTimer(duration * 2f);
            Ray aimRay = GetAimRay();

            if (isAuthority)
            {
                BulletAttack bulet = new BulletAttack
                {
                    owner = gameObject,
                    weapon = gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    damage = tickDamageCoefficient * damageStat,
                    force = force,
                    muzzleName = muzzleString,
                    hitEffectPrefab = impactEffectPrefab,
                    isCrit = RollCrit(), 
                    radius = radius,
                    falloffModel = BulletAttack.FalloffModel.None,
                    stopperMask = LayerIndex.world.mask,
                    procCoefficient = tickProcCoefficient,
                    maxDistance = maxDistance,
                    smartCollision = true,
                };
                bulet.Fire();
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if ((stopwatch >= entryDuration && !hasBegunFlamethrower))
            {
                //Debug.Log("entering flamethrower");
                hasBegunFlamethrower = true;
                Fire(muzzleString);
            }

            if (hasBegunFlamethrower)
            {
                flamethrowerStopwatch += Time.fixedDeltaTime;
                float tickRate = baseTickFrequency / attackSpeedStat;
                if (flamethrowerStopwatch > tickRate)
                {
                    //Debug.Log("ticking flamethrower");
                    flamethrowerStopwatch -= tickRate;
                    Fire(muzzleString);
                }
            }

            if (stopwatch >= baseDuration && !inputBank.skill1.down && isAuthority)
            {
                outer.SetNextStateToMain();
                //Debug.Log("exiting flamethrower");
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
