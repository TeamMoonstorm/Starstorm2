using RoR2;
using UnityEngine;

namespace EntityStates.NemToolbot
{
    public class FireShotgun : BaseSkillState
    {
        public static float damageCoefficient = 1.2f;
        public static float procCoefficient = 0.5f;
        public static float baseDuration = 0.6f;
        public static float force = 300f;
        public static float recoilAmplitude = 3f;
        public static float spreadBloomValue = 1.5f;
        public static int pelletCount = 6;
        public static float spreadAngle = 10f;
        public static float maxDistance = 12f;
        public static float bulletRadius = 0.3f;

        public static string muzzleString = "Muzzle";
        public static string soundString = "";
        public static GameObject muzzleFlashPrefab;
        public static GameObject tracerEffectPrefab;
        public static GameObject hitEffectPrefab;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            characterBody.SetAimTimer(duration + 1f);

            Util.PlaySound(soundString, gameObject);
            if (muzzleFlashPrefab != null)
            {
                EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, muzzleString, transmit: false);
            }

            PlayCrossfade("Gesture, Override", "FireShotgun", "FireShotgun.playbackRate", duration, 0.05f);
            AddRecoil(-2f * recoilAmplitude, -3f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);

            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                DamageTypeCombo damageType = DamageType.Generic;
                damageType.damageSource = DamageSource.Primary;

                BulletAttack bulletAttack = new BulletAttack
                {
                    bulletCount = (uint)pelletCount,
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = damageCoefficient * damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = damageType,
                    falloffModel = BulletAttack.FalloffModel.Buckshot,
                    maxDistance = maxDistance,
                    force = force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = spreadAngle,
                    isCrit = RollCrit(),
                    owner = gameObject,
                    muzzleName = muzzleString,
                    smartCollision = true,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    radius = bulletRadius,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = tracerEffectPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    hitEffectPrefab = hitEffectPrefab
                };
                bulletAttack.Fire();
            }

            characterBody.AddSpreadBloom(spreadBloomValue);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
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
