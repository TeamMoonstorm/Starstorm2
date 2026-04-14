using RoR2;
using UnityEngine;

namespace EntityStates.NemToolbot
{
    public class FireSniperLaser : BaseSkillState
    {
        public static float damageCoefficient = 6f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.8f;
        public static float force = 2500f;
        public static float recoilAmplitude = 4f;
        public static float spreadBloomValue = 2f;
        public static float maxDistance = 1000f;
        public static float bulletRadius = 0.5f;

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
            characterBody.SetAimTimer(duration + 2f);

            Util.PlaySound(soundString, gameObject);
            if (muzzleFlashPrefab != null)
            {
                EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, muzzleString, transmit: false);
            }

            PlayCrossfade("Gesture, Override", "FireSniper", "FireSniper.playbackRate", duration, 0.05f);
            AddRecoil(-2f * recoilAmplitude, -3f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);

            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                DamageTypeCombo damageType = DamageType.Generic;
                damageType.damageSource = DamageSource.Primary;

                BulletAttack bulletAttack = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = damageCoefficient * damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = damageType,
                    falloffModel = BulletAttack.FalloffModel.None,
                    maxDistance = maxDistance,
                    force = force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = 0f,
                    isCrit = RollCrit(),
                    owner = gameObject,
                    muzzleName = muzzleString,
                    smartCollision = true,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    radius = bulletRadius,
                    sniper = true,
                    stopperMask = LayerIndex.world.mask,
                    weapon = null,
                    tracerEffectPrefab = tracerEffectPrefab,
                    spreadPitchScale = 0f,
                    spreadYawScale = 0f,
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
            return InterruptPriority.PrioritySkill;
        }
    }
}
