using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemCaptain
{
    public class Haidara : BaseState
    {
        public static float dmgCoef;
        public static float force;
        public static GameObject hitEffectPrefab;
        public static float radius;
        public static float procCoef;
        public static float maxDistance;
        public static float baseDuration;
        public static string muzzleString;
        public static GameObject projectilePrefab;
        private float duration;
        private bool hasFired = false;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
        }
        public void FireThunder()
        {
            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                bool isCrit = RollCrit();
                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.position = characterBody.transform.position;
                fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
                fireProjectileInfo.crit = isCrit;
                fireProjectileInfo.damage = dmgCoef * damageStat;
                fireProjectileInfo.owner = gameObject;
                fireProjectileInfo.damageTypeOverride = DamageType.Shock5s;
                fireProjectileInfo.projectilePrefab = projectilePrefab;
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }

            /*Ray aimRay = GetAimRay();
            bool isCrit = RollCrit();
            if (isAuthority)
            {
                new BulletAttack
                {
                    owner = gameObject,
                    weapon = gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    damage = dmgCoef * damageStat,
                    force = force,
                    muzzleName = muzzleString,
                    hitEffectPrefab = hitEffectPrefab,
                    isCrit = isCrit,
                    radius = radius,
                    falloffModel = BulletAttack.FalloffModel.None,
                    stopperMask = LayerIndex.world.mask,
                    procCoefficient = procCoef,
                    maxDistance = maxDistance,
                    smartCollision = true,
                    damageType = DamageType.Shock5s
                }.Fire();
            }*/
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration * 0.6f && !hasFired)
            {
                hasFired = true;
                FireThunder();
            }
            if (fixedAge >= duration)
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
