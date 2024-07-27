using RoR2;
using SS2.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Pyro
{
    public class FireFlamethrower : BaseState
    {
        public static float baseDuration;
        public static float baseTickFrequency;
        public static float baseEntryDuration;

        private float tickRate;
        private float stopwatch;
        private float flamethrowerStopwatch;
        private float duration;
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

        private PyroController pc;
        private ChildLocator childLocator;
        private ParticleSystem flames;

        public override void OnEnter()
        {
            base.OnEnter();

            pc = GetComponent<PyroController>();

            stopwatch = 0f;

            duration = baseDuration / attackSpeedStat;
            entryDuration = baseEntryDuration / attackSpeedStat;
            tickRate = baseTickFrequency / attackSpeedStat;

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
                flames = childLocator.FindChild("Flames").GetComponent<ParticleSystem>();
            }

            //playanimation
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= entryDuration && !hasBegunFlamethrower)
            {
                hasBegunFlamethrower = true;
                Fire(muzzleString);
            }

            if (hasBegunFlamethrower)
            {
                flamethrowerStopwatch += Time.fixedDeltaTime;
                float tickRate = 1f / baseTickFrequency / attackSpeedStat;
                if (flamethrowerStopwatch > tickRate)
                {
                    flamethrowerStopwatch -= tickRate;
                    Fire(muzzleString);
                }
            }

            if (stopwatch >= baseDuration && !inputBank.skill1.down && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire(string muzzleString)
        {
            DamageType damageType;

            if (pc.heat >= heatIgniteThreshold)
                damageType = (Util.CheckRoll(igniteChanceHighHeat, characterBody.master) ? DamageType.IgniteOnHit : DamageType.Generic);
                //to-do: unique damage type that scales ignite chance based on range / staged heat levels?

            else
                damageType = DamageType.Generic;

            Ray aimRay = GetAimRay();
            if (isAuthority)
            {
                new BulletAttack
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
                    isCrit = false, //to-do: make crits come in short bursts like tf2
                    radius = radius,
                    falloffModel = BulletAttack.FalloffModel.None,
                    stopperMask = LayerIndex.world.mask,
                    procCoefficient = tickProcCoefficient,
                    maxDistance = maxDistance,
                    smartCollision = true,
                    damageType = damageType,
                }.Fire();

                if (characterMotor)
                    base.characterMotor.ApplyForce(aimRay.direction * -recoilForce, false, false);

                if (pc)
                    pc.AddHeat(heatPerTick);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
