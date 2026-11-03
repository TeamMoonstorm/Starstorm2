using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Lancer
{
    public class ThrowSpear : BaseSkillState
    {
        public static float baseDuration = 0.5f;
        public static float damageCoefficient = 4f;
        public static float force = 40f;
        public static float procCoefficient = 1f;
        public static GameObject projectilePrefab;
        public static GameObject muzzleflashEffectPrefab;
        public static string throwSoundString = "";
        public static string muzzleString = "SpearMuzzle";

        private float duration;
        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            StartAimMode(2f);
            PlayCrossfade("Gesture, Override", "ThrowSpear", "ThrowSpear.playbackRate", duration, 0.1f);
            Util.PlayAttackSpeedSound(throwSoundString, gameObject, attackSpeedStat);

            if (muzzleflashEffectPrefab)
                EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, gameObject, muzzleString, false);

            FireSpear();
        }

        private void FireSpear()
        {
            if (hasFired)
                return;
            hasFired = true;

            if (isAuthority && projectilePrefab)
            {
                Ray aimRay = GetAimRay();
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = projectilePrefab,
                    position = FindModelChild(muzzleString)?.position ?? aimRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    owner = gameObject,
                    damage = damageStat * damageCoefficient,
                    force = force,
                    crit = RollCrit(),
                    damageTypeOverride = new DamageTypeCombo(DamageType.SlowOnHit, DamageTypeExtended.Generic, DamageSource.Secondary)
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }

            if (gameObject.TryGetComponent(out SS2.Components.LancerController lancerController))
            {
                if (NetworkServer.active)
                {
                    lancerController.SetSpearState(SS2.Components.LancerController.SpearState.Thrown);
                }
            }
            else
            {
                Debug.LogError("ThrowSpear: Failed to get LancerController component.");
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
