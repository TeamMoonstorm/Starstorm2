using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using R2API;

namespace EntityStates.NemCroco
{
    public class FireDiseaseProjectile : BaseSkillState
    {
        public static GameObject projectilePrefab;
        public static GameObject effectPrefab;

        private static float baseDuration = 0.6f;
        private static float damageCoefficient = 2.5f;
        private static float force = 200f;
        private static string attackString = "Play_acrid_R_shoot";
        private static float recoilAmplitude = 2f;
        private static float bloom = 1f;
        private static string muzzleString = "MouthMuzzle";

        private float duration;
        
        public override void OnEnter()
        {
            base.OnEnter();

            Ray aimRay = GetAimRay();
            TrajectoryAimAssist.ApplyTrajectoryAimAssist(ref aimRay, projectilePrefab, gameObject, 1f);
            duration = baseDuration / attackSpeedStat;

            StartAimMode(duration + 2f, false);
            PlayAnimation("Gesture, Mouth", "FireSpit", "FireSpit.playbackRate", duration);
            Util.PlaySound(attackString, gameObject);
            AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);

            characterBody.AddSpreadBloom(bloom);
            if (effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, gameObject, muzzleString, false);
            }
            if (isAuthority)
            {
                DamageTypeCombo damageType = DamageTypeCombo.GenericSpecial;
                damageType.AddModdedDamageType(SS2.Survivors.NemCroco.DamageShareOnHit);

                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.projectilePrefab = projectilePrefab;
                fireProjectileInfo.position = aimRay.origin;
                fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
                fireProjectileInfo.owner = gameObject;
                fireProjectileInfo.damage = damageStat * damageCoefficient;
                fireProjectileInfo.damageTypeOverride = damageType;
                fireProjectileInfo.force = force;
                fireProjectileInfo.crit = RollCrit();
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration)
            {
                if (isAuthority)
                {
                    outer.SetNextStateToMain();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        
    }
}
