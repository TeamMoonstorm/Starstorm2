using R2API;
using RoR2;
using RoR2.Projectile;
using SS2.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Pyro
{
    public class FireFlamethrower : BaseState
    {
        public static float baseDuration;
        public static float baseTickFrequency;
        public static float baseEntryDuration;
        public static float pressureDuration;

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
        public static string smokeMuzzleEffectString;

        public static GameObject flameEffectPrefab;
        public static GameObject projectilePrefab;

        private Transform flamethrowerTransform;

        private PyroController pc;
        private ChildLocator childLocator;
        private ParticleSystem flames;

        private ScaleParticleSystemDuration smokeMuzzleEffect;

        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.isSprinting = false;

            pc = GetComponent<PyroController>();

            stopwatch = 0f;

            hasBegunFlamethrower = false;

            duration = baseDuration / attackSpeedStat;
            entryDuration = baseEntryDuration / attackSpeedStat;
            tickRate = baseTickFrequency / attackSpeedStat;

            characterBody.SetAimTimer(duration * 2f);

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
                //flames = childLocator.FindChild("Flames").GetComponent<ParticleSystem>();
                smokeMuzzleEffect = childLocator.FindChild(smokeMuzzleEffectString).GetComponent<ScaleParticleSystemDuration>();
            }

            Util.PlaySound("Play_pyro_primary_start", gameObject);

            if (smokeMuzzleEffect)
            {
                smokeMuzzleEffect.newDuration = entryDuration;
                smokeMuzzleEffect.UpdateDuration();
            }

            PlayCrossfade("Gesture, Override", "FirePrimary", 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if ((stopwatch >= entryDuration && !hasBegunFlamethrower || HasBuff(SS2.SS2Content.Buffs.bdPyroPressure)) && !hasBegunFlamethrower)
            {
                //Debug.Log("entering flamethrower");
                hasBegunFlamethrower = true;
                // silly visual flamethrower for sake of making it look fuller
                flamethrowerTransform = Object.Instantiate(flameEffectPrefab, childLocator.FindChild(muzzleString)).transform;
                Util.PlaySound("Play_pyro_primary_loop", gameObject);
                Fire(muzzleString);
            }

            if (hasBegunFlamethrower)
            {
                flamethrowerStopwatch += Time.fixedDeltaTime;
                float tickRate = baseTickFrequency / attackSpeedStat;
                while (flamethrowerStopwatch > tickRate)
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
            characterBody.AddTimedBuffAuthority(SS2.SS2Content.Buffs.bdPyroPressure.buffIndex, pressureDuration);
            Util.PlaySound("Stop_pryo_primary_loop", gameObject); //
            Util.PlaySound("Play_pyro_primary_end", gameObject);
            PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);
            if (flamethrowerTransform)
                Destroy(flamethrowerTransform.gameObject);
        }

        private void Fire(string muzzleString)
        {
            characterBody.SetAimTimer(2f);

            Ray aimRay = GetAimRay();
            if (isAuthority)
            {
                DamageTypeCombo dtc = new DamageTypeCombo();
                dtc.damageSource = DamageSource.Primary;
                dtc.damageTypeExtended = DamageTypeExtended.FireNoIgnite;
                dtc.AddModdedDamageType(SS2.Survivors.Pyro.FlamethrowerDamageType);

                ProjectileManager.instance.FireProjectile(
                    projectilePrefab,
                    aimRay.origin,
                    Util.QuaternionSafeLookRotation(aimRay.direction),
                    gameObject,
                    tickDamageCoefficient * damageStat,
                    force,
                    RollCrit(),
                    DamageColorIndex.Default,
                    null,
                    -1,
                    dtc
                    );

                if (flamethrowerTransform)
                {
                    flamethrowerTransform.forward = aimRay.direction;
                }

                if (pc)
                {
                    pc.AddHeat(heatPerTick);
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
