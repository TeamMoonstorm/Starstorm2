using RoR2;
using SS2.Components;
using UnityEngine;
using UnityEngine.Networking;

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
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            characterBody.SetAimTimer(2f);

            if (!gameObject.TryGetComponent(out controller))
            {
                Debug.LogError("FireRapidLaser: NemToolbotController not found.");
            }

            PlayCrossfade("Gesture, Override", "FireRapidLaser", "FireRapidLaser.playbackRate", duration, 0.05f);
            FireBullet();
        }

        private void FireBullet()
        {
            fireTimer = baseFireInterval / attackSpeedStat;

            if (isAuthority && controller != null && NetworkServer.active && !controller.TryConsumeAmmo(NemToolbotController.WeaponType.RapidLaser))
            {
                outer.SetNextStateToMain();
                return;
            }

            Util.PlaySound(soundString, gameObject);
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
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            fireTimer -= GetDeltaTime();
            if (fireTimer <= 0f)
            {
                FireBullet();
            }

            if (isAuthority && !IsKeyDownAuthority())
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
