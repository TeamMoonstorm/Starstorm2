using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.NemToolbot
{
    public class FireGrenadeLauncher : BaseSkillState
    {
        public static float damageCoefficient = 3f;
        public static float force = 1500f;
        public static float baseDuration = 0.6f;
        public static float recoilAmplitude = 1.5f;
        public static float spreadBloomValue = 1f;

        public static string muzzleString = "Muzzle";
        public static string soundString = "";
        public static GameObject muzzleFlashPrefab;
        public static GameObject projectilePrefab;

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

            PlayCrossfade("Gesture, Override", "FireGrenade", "FireGrenade.playbackRate", duration, 0.05f);
            AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);

            if (isAuthority)
            {
                Ray aimRay = GetAimRay();

                if (projectilePrefab != null)
                {
                    DamageTypeCombo damageType = DamageType.Generic;
                    damageType.damageSource = DamageSource.Primary;

                    FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                    fireProjectileInfo.projectilePrefab = projectilePrefab;
                    fireProjectileInfo.position = aimRay.origin;
                    fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
                    fireProjectileInfo.owner = gameObject;
                    fireProjectileInfo.damage = damageStat * damageCoefficient;
                    fireProjectileInfo.force = force;
                    fireProjectileInfo.crit = RollCrit();
                    fireProjectileInfo.damageTypeOverride = damageType;

                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
                else
                {
                    Debug.LogError("NemToolbot FireGrenadeLauncher: projectilePrefab is null.");
                }
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
