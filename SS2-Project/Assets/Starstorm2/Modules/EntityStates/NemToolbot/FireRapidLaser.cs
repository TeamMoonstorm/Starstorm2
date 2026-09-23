using RoR2;
using SS2;
using SS2.Components;
using UnityEngine;

namespace EntityStates.NemToolbot
{
    public class FireRapidLaser : BaseSkillState
    {
        public static float damageCoefficient = 0.8f;
        public static float procCoefficient = 0.7f;
        public static float baseFireInterval = 0.12f;
        public static float force = 100f;
        public static float recoilAmplitude = 0.4f;
        public static float spreadBloomValue = 0.3f;
        public static float maxDistance = 60f;
        public static float bulletRadius = 0.4f;
        public static float baseDuration = 0.1f;

        public static string muzzleString = "Muzzle";
        public static string soundString = "";
        public static GameObject muzzleFlashPrefab;
        public static GameObject tracerEffectPrefab;
        public static GameObject hitEffectPrefab;

        private NemToolbotController controller;
        private float fireTimer;
        // private float duration;
        private float fireInterval;

        public override void OnEnter()
        {
            base.OnEnter();
            // duration = baseDuration / attackSpeedStat;
            fireInterval = baseFireInterval / attackSpeedStat;

            if (!(fireInterval > 0f) || float.IsInfinity(fireInterval))
            {
                SS2Log.Error("FireRapidLaser: Fire interval must be finite and positive.");
                if (isAuthority)
                    outer.SetNextStateToMain();
                return;
            }

            if (!gameObject.TryGetComponent(out controller))
            {
                SS2Log.Error("FireRapidLaser: NemToolbotController not found.");
                if (isAuthority)
                    outer.SetNextStateToMain();
                return;
            }

            // PlayCrossfade("Gesture, Override", "FireRapidLaser", "FireRapidLaser.playbackRate", duration, 0.05f);
            FireBullet();
            fireTimer = fireInterval;
        }

        private bool FireBullet()
        {
            if (isAuthority && !controller.TryConsumeAmmo(NemToolbotController.WeaponType.RapidLaser))
            {
                outer.SetNextStateToMain();
                return false;
            }

            characterBody.SetAimTimer(2f);
            // Util.PlaySound(soundString, gameObject);
            if (muzzleFlashPrefab != null)
            {
                EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, muzzleString, transmit: false);
            }

            AddRecoil(-0.5f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);

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
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    maxDistance = maxDistance,
                    force = force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = characterBody.spreadBloomAngle,
                    isCrit = RollCrit(),
                    owner = gameObject,
                    muzzleName = muzzleString,
                    smartCollision = true,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    radius = bulletRadius,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = gameObject,
                    tracerEffectPrefab = tracerEffectPrefab,
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    hitEffectPrefab = hitEffectPrefab
                };
                bulletAttack.Fire();
            }

            characterBody.AddSpreadBloom(spreadBloomValue);
            return true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!controller)
                return;

            if (isAuthority && !IsKeyDownAuthority())
            {
                outer.SetNextStateToMain();
                return;
            }

            fireTimer -= GetDeltaTime();
            int remainingShots = NemToolbotController.GetMaxAmmo(NemToolbotController.WeaponType.RapidLaser);
            while (fireTimer <= 0f && remainingShots-- > 0)
            {
                if (!FireBullet())
                    return;
                fireTimer += fireInterval;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
