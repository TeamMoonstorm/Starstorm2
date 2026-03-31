using R2API;
using RoR2;
using RoR2.Projectile;
using SS2.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Pyro
{
    public class FireFlamethrower : BaseState
    {
        public static float baseTickFrequency;
        public static float baseEntryDuration;
        public static float pressureDuration;

        private float tickRate;
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
        public static string smokeMuzzleEffectString;
        private static float spreadBloonPerSecond = 1f;
        private static float igniteChance = 50f;

        public static GameObject flameEffectPrefab;
        public static GameObject projectilePrefab;

        private Transform flamethrowerTransform;

        private PyroController pc;
        private ChildLocator childLocator;
        private ParticleSystem flames;

        private ScaleParticleSystemDuration smokeMuzzleEffect;

        private EntityStateMachine jetpackStateMachine;

        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.isSprinting = false;

            pc = GetComponent<PyroController>();

            stopwatch = 0f;

            hasBegunFlamethrower = false;

            entryDuration = baseEntryDuration / attackSpeedStat;
            tickRate = baseTickFrequency / attackSpeedStat;

            characterBody.SetAimTimer(2f);

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
                smokeMuzzleEffect = childLocator.FindChild(smokeMuzzleEffectString).GetComponent<ScaleParticleSystemDuration>();
            }

            Util.PlaySound("Play_pyro_primary_start", gameObject);

            if (smokeMuzzleEffect)
            {
                smokeMuzzleEffect.newDuration = entryDuration;
                smokeMuzzleEffect.UpdateDuration();
            }

            PlayCrossfade("Gesture, Override", "FirePrimary", 0.1f);

            // fake "agile", only while hovering.
            jetpackStateMachine = FindSiblingStateMachine("Jetpack");
            if (jetpackStateMachine && jetpackStateMachine.state is not PyroHoverpack)
            {
                characterBody.isSprinting = false;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (jetpackStateMachine)
            {
                if (characterBody.isSprinting && jetpackStateMachine.state is not PyroHoverpack)
                {
                    outer.SetNextStateToMain();
                }
            }

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
                characterBody.AddSpreadBloom(spreadBloonPerSecond * Time.fixedDeltaTime);

                flamethrowerStopwatch += Time.fixedDeltaTime;
                float tickRate = baseTickFrequency / attackSpeedStat;
                while (flamethrowerStopwatch > tickRate)
                {
                    flamethrowerStopwatch -= tickRate;
                    Fire(muzzleString);
                }
            }

            if (!inputBank.skill1.down && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(SS2.SS2Content.Buffs.bdPyroPressure, pressureDuration);
            }
            
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
                DamageTypeCombo damageType = new DamageTypeCombo();
                damageType.damageSource = DamageSource.Primary;
                damageType.damageTypeExtended = DamageTypeExtended.FireNoIgnite;
                
                if (pc && pc.isHighHeat && Util.CheckRoll(igniteChance, characterBody.master))
                {
                    damageType.AddModdedDamageType(SS2.Survivors.Pyro.PyroIgniteOnHit);
                }

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
                    damageType
                    );

                if (flamethrowerTransform)
                {
                    flamethrowerTransform.forward = aimRay.direction;
                }

                if (pc)
                {
                    pc.AddHeat(heatPerTick); //TODO: AddHeat is server only bruh!!
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
