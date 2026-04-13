using R2API;
using RoR2;
using RoR2.Projectile;
using SS2;
using SS2.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Pyro
{
    public class FireFlamethrower : BaseState
    {
        private static float ticksPerSecond = 6f;
        private static float baseEntryDuration = 0.3f;
        private static float pressureDuration = 0.1f;

        private float baseTickRate;
        private float tickRate;
        private float stopwatch;
        private float flamethrowerStopwatch;
        private float entryDuration;

        private bool hasBegunFlamethrower;

        private static float heatPerSecond = 6.25f;
        private static float procCoefficientPerSecond = 5f;
        private static float damageCoefficientPerSecond = 2.7f;
        private static float spreadBloonPerSecond = 1f;
        private static float igniteChance = 50f;
        private static float force = 250f;

        private static string muzzleString = "Muzzle";
        private static string smokeMuzzleEffectString = "SmokeEffect";
        

        public static GameObject flameEffectPrefab;
        public static GameObject beamEffectPrefab;
        public static GameObject projectilePrefab;

        private Transform flamethrowerTransform;
        private Transform beamTransform;

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

            entryDuration = baseEntryDuration / attackSpeedStat;
            baseTickRate = 1f / ticksPerSecond;
            tickRate = baseTickRate / attackSpeedStat;

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
            if (!characterBody.HasBuff(SS2Content.Buffs.bdPyroJet))
            {
                if (!characterBody.HasBuff(SS2Content.Buffs.BuffWatchMetronome)) // watchmetronome also adds fake agile. TODO i guess: make a util for fake agility
                    characterBody.isSprinting = false;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                if (characterBody.isSprinting)
                {
                    if (!characterBody.HasBuff(SS2Content.Buffs.bdPyroJet) && !characterBody.HasBuff(SS2Content.Buffs.BuffWatchMetronome))
                    {
                        outer.SetNextStateToMain();
                    }
                    else
                    {
                        characterDirection.moveVector = inputBank.aimDirection; // if we are sprinting while firing, stay facing forward
                    }
                }
            }


            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= entryDuration || HasBuff(SS2Content.Buffs.bdPyroPressure))
            {
                if (!hasBegunFlamethrower)
                {
                    BeginFlamethrower();

                    Util.PlaySound("Play_pyro_primary_loop", gameObject);
                    Fire();
                }
            }

            if (hasBegunFlamethrower)
            {
                characterBody.AddSpreadBloom(spreadBloonPerSecond * Time.fixedDeltaTime);

                flamethrowerStopwatch += Time.fixedDeltaTime;
                int fuckee = 0;
                while (flamethrowerStopwatch > tickRate && fuckee <= 3) //if youre firing 3 projectiles per frame, youve got bigger problems than a dps loss
                {
                    fuckee++;
                    flamethrowerStopwatch -= tickRate;
                    Fire();
                }
            }

            if (!inputBank.skill1.down && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }
        private void BeginFlamethrower()
        {
            //Debug.Log("entering flamethrower");
            hasBegunFlamethrower = true;
            // silly visual flamethrower for sake of making it look fuller
            if (flameEffectPrefab)
                flamethrowerTransform = GameObject.Instantiate(flameEffectPrefab, childLocator.FindChild(muzzleString)).transform;
            if (beamEffectPrefab)
                beamTransform = GameObject.Instantiate(beamEffectPrefab, childLocator.FindChild(muzzleString)).transform;

        }
        public override void Update()
        {
            base.Update();

            if (flamethrowerTransform)
            {

            }
            if (beamTransform)
            {
                beamTransform.transform.forward = inputBank.aimDirection;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(SS2Content.Buffs.bdPyroPressure, pressureDuration);
            }
            
            Util.PlaySound("Stop_pryo_primary_loop", gameObject); //
            Util.PlaySound("Play_pyro_primary_end", gameObject);
            PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);
            if (flamethrowerTransform)
            {
                Destroy(flamethrowerTransform.gameObject);
            }
            if (beamTransform)
            {
                Destroy(beamTransform.gameObject);
            }
        }

        private void Fire()
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
                float damage = damageCoefficientPerSecond * baseTickRate * damageStat;
                float procCoefficient = procCoefficientPerSecond * baseTickRate; // wait what the fuck do you mean FireProjectileInfo doesnt have proc coefficient ????

                ProjectileManager.instance.FireProjectile(
                    projectilePrefab,
                    aimRay.origin,
                    Util.QuaternionSafeLookRotation(aimRay.direction),
                    gameObject,
                    damage,
                    force,
                    RollCrit(),
                    DamageColorIndex.Default,
                    null,
                    -1,
                    damageType
                    );

                if (pc)
                {
                    pc.AddHeat(heatPerSecond * baseTickRate);
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
