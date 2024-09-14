﻿using RoR2;
using SS2;
using SS2.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Pyro
{
    public class SuppressiveFire : BaseState
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

        public static GameObject impactEffectPrefab;
        public static GameObject flameEffectPrefab;

        private Transform flamethrowerTransform;

        private PyroController pc;
        private ChildLocator childLocator;
        private ParticleSystem flames;

        public CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;
        private CharacterCameraParamsData chargeCameraParams = new CharacterCameraParamsData
        {
            maxPitch = 85f,
            minPitch = -85f,
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = cameraPos,
            wallCushion = 0.1f,
        };

        [HideInInspector]
        public static Vector3 cameraPos = new Vector3(0f, -0.1f, -7.2f);

        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.isSprinting = false;

            pc = GetComponent<PyroController>();

            cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.2f);

            CameraTargetParams.CameraParamsOverrideRequest request = new CameraTargetParams.CameraParamsOverrideRequest
            {
                cameraParamsData = chargeCameraParams,
                priority = 0f
            };

            camOverrideHandle = cameraTargetParams.AddParamsOverride(request, 0.2f);

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
            }

            //impactEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/MissileExplosionVFX.prefab").WaitForCompletion();

            //playanimation
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if ((stopwatch >= entryDuration && !hasBegunFlamethrower || HasBuff(SS2.SS2Content.Buffs.bdPyroPressure)) && !hasBegunFlamethrower)
            {
                //Debug.Log("entering flamethrower");
                hasBegunFlamethrower = true;
                flamethrowerTransform = Object.Instantiate(flameEffectPrefab, childLocator.FindChild(muzzleString)).transform;
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

            if (stopwatch >= baseDuration && (!inputBank.skill2.down || pc.heat < heatPerTick * -1f) && isAuthority)
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
            if (cameraTargetParams)
            {
                cameraTargetParams.RemoveParamsOverride(camOverrideHandle, 0.7f);
            }
            if (flamethrowerTransform)
                Destroy(flamethrowerTransform.gameObject);
        }

        private void Fire(string muzzleString)
        {
            DamageType damageType;

            characterBody.SetAimTimer(duration * 2f);

            float damage = tickDamageCoefficient * damageStat;

            //Debug.Log("Firing");

            if (pc.heat >= heatIgniteThreshold)
            {
                damage *= 1.5f;
            }

            damageType = (Util.CheckRoll(igniteChanceHighHeat, characterBody.master) ? DamageType.IgniteOnHit : DamageType.Generic);

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
                    isCrit = RollCrit(), //to-do: make crits come in short bursts like tf2
                    radius = radius,
                    falloffModel = BulletAttack.FalloffModel.None,
                    stopperMask = LayerIndex.world.mask,
                    procCoefficient = tickProcCoefficient,
                    maxDistance = maxDistance,
                    smartCollision = true,
                    damageType = damageType,
                }.Fire();

                if (flamethrowerTransform)
                    flamethrowerTransform.forward = aimRay.direction;

                if (characterMotor)
                    base.characterMotor.ApplyForce(aimRay.direction * -recoilForce, false, false);

                if (pc)
                    pc.AddHeat(heatPerTick);

                //Debug.Log("Fired");
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
