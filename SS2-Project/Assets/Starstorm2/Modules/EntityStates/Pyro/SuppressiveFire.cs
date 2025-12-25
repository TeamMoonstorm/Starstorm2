using RoR2;
using RoR2.Projectile;
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
        [Tooltip("The ratio of how many per second should ignite. Hard to explain :(")]
        public static float igniteFrequencyCoefficient;

        private float tickRate;
        private float stopwatch;
        private float heatStopwatch;
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
        public static float recoil;
        public static float spreadBloom;
        public static string muzzleString;

        public static GameObject projectilePrefab;
        public static GameObject ignitePrefab;
        public static GameObject impactEffectPrefab;
        public static GameObject flameEffectPrefab;

        private float igniteFrequency;
        private float fired;

        private Transform flamethrowerTransform;

        private PyroController pc;
        private ChildLocator childLocator;
        private ParticleSystem flames;

        public CameraTargetParams.CameraParamsOverrideHandle camOverrideHandle;

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

            PlayCrossfade("Gesture, Override", "FireSecondary", 0.1f);

            // keeps it roughly ~3 fire pools a second, regardless of tick rate, i hope
            igniteFrequency = igniteFrequencyCoefficient / tickRate;

            characterBody.SetAimTimer(duration * 2f);

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
                //flames = childLocator.FindChild("Flames").GetComponent<ParticleSystem>();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if ((stopwatch >= entryDuration && !hasBegunFlamethrower) || (HasBuff(SS2.SS2Content.Buffs.bdPyroPressure) && !hasBegunFlamethrower))
            {
                //Debug.Log("entering flamethrower");
                hasBegunFlamethrower = true;
                // flamethrowerTransform = Object.Instantiate(flameEffectPrefab, childLocator.FindChild(muzzleString)).transform;
                Fire(muzzleString);
            }

            if (hasBegunFlamethrower)
            {
                heatStopwatch += Time.fixedDeltaTime;
                flamethrowerStopwatch += Time.fixedDeltaTime;
                // we recalculate this each time to accomodate for attack speed changes during
                tickRate = baseTickFrequency / attackSpeedStat;
                if (flamethrowerStopwatch > tickRate)
                {
                    //Debug.Log("ticking flamethrower");
                    flamethrowerStopwatch -= tickRate;
                    Fire(muzzleString);
                }
                // we count this separate for fuel burn rate consistency
                if (heatStopwatch > baseTickFrequency && pc != null)
                {
                    pc.AddHeat(heatPerTick);
                    heatStopwatch -= baseTickFrequency;
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
            PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);
            characterBody.AddTimedBuffAuthority(SS2.SS2Content.Buffs.bdPyroPressure.buffIndex, pressureDuration);

            if (flamethrowerTransform != null)
            {
                Destroy(flamethrowerTransform.gameObject);
            }
        }

        private void Fire(string muzzleString)
        {
            characterBody.SetAimTimer(duration * 2f);

            float damage = tickDamageCoefficient * damageStat;
            DamageColorIndex color = DamageColorIndex.Default;

            GameObject prefab = projectilePrefab;

            if (fired >= igniteFrequency)
            {
                prefab = ignitePrefab;
                fired = 0;
            }

            DamageTypeCombo dtc = new DamageTypeCombo();
            dtc.damageType = DamageType.IgniteOnHit;
            dtc.damageSource = DamageSource.Secondary;

            if (pc.heat >= heatIgniteThreshold)
            {
                damage *= 1.5f;
                color = DamageColorIndex.WeakPoint;
            }

            Ray aimRay = GetAimRay();

            // spread calc
            {
                Vector3 up = Vector3.up;
                Vector3 right = Vector3.Cross(up, aimRay.direction);

                float deviation = Random.Range(0f, characterBody.spreadBloomAngle);
                float roll = Random.Range(0.0f, 360.0f);

                Vector3 spreadXZ = Quaternion.Euler(0.0f, 0.0f, roll) * (Quaternion.Euler(deviation, 0.0f, 0.0f) * Vector3.forward);
                float spreadY = spreadXZ.y;
                spreadXZ.y = 0f;
                float yaw = (Mathf.Atan2(spreadXZ.z, spreadXZ.x) * Mathf.Rad2Deg - 90.0f);
                float pitch = (Mathf.Atan2(spreadY, spreadXZ.magnitude) * Mathf.Rad2Deg);

                aimRay = new Ray(origin: aimRay.origin, direction: Quaternion.AngleAxis(yaw, up) * (Quaternion.AngleAxis(pitch, right) * aimRay.direction));
            }

            if (isAuthority)
            {
                ProjectileManager.instance.FireProjectile(
                    prefab,
                    aimRay.origin,
                    Util.QuaternionSafeLookRotation(aimRay.direction),
                    gameObject,
                    damage,
                    force,
                    RollCrit(),
                    color,
                    null,
                    -1,
                    dtc
                    );
            }

            AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);

            if (flamethrowerTransform)
                flamethrowerTransform.forward = aimRay.direction;

            fired++;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
