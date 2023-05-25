using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemHuntress2
{
    public class BaseFireArrow : BaseSkillState
    {
        public float charge;
        public GameObject projectilePrefab;

        public float maxDamageCoefficient;
        public float minDamageCoefficient;
        public float procCoefficient;
        public float minRecoil;
        public float maxRecoil;
        public float minProjectileSpeed;
        public float maxProjectileSpeed;

        public float baseDuration;

        private float damageCoefficient;
        private float recoil;
        private float projectileSpeed;
        private float duration;
        private float fireDuration;
        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();

            if (projectilePrefab == null)
            {
                Debug.Log("nemhuntress arrow projectile prefab is null!");
                return;
            }

            characterBody.SetAimTimer(2f);
            hasFired = false;
            duration = baseDuration / attackSpeedStat;
            damageCoefficient = Util.Remap(charge, 0f, 1f, minDamageCoefficient, maxDamageCoefficient);
            recoil = Util.Remap(charge, 0f, 1f, minRecoil, maxRecoil);
            projectileSpeed = Util.Remap(charge, 0f, 1f, minProjectileSpeed, maxProjectileSpeed);
            fireDuration = 0.4f * duration;
        }

        public virtual void FireProjectile()
        {
            if (!hasFired)
            {
                hasFired = true;

                if (isAuthority)
                {
                    float damage = damageCoefficient * damageStat;
                    AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
                    characterBody.AddSpreadBloom(0.33f * recoil);
                    Ray aimRay = GetAimRay();

                    ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), gameObject, damage, 0f, RollCrit(), DamageColorIndex.Default, null, projectileSpeed);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireDuration)
            {
                FireProjectile();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
