using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using System;
using RoR2.Projectile;
using Moonstorm.Starstorm2;
using R2API;
using Moonstorm.Starstorm2.DamageTypes;
using static R2API.DamageAPI;

namespace EntityStates.MULE
{
    public class MULEAimNet : BaseState
    {
        public static GameObject projectilePrefab;
        public static float baseDuration;
        public static float damageCoefficient;
        public static string enterSoundString;
        public static string muzzleName;
        public static GameObject muzzleFlashprefab;

        private float duration;
        private GameObject projectileInstance;
        private ProjectileImpactExplosion pie;
        private ProjectileDamage pd;

        public override void OnEnter()
        {
            base.OnEnter();
            //EffectManager.SimpleMuzzleFlash
            duration = baseDuration / attackSpeedStat;
            Util.PlaySound(enterSoundString, gameObject);
            PlayCrossfade("Gesture, Override", "ShootNet", 0.05f);

            projectileInstance = projectilePrefab;
            Debug.Log("projectile instance: " + projectileInstance);
            //pie = projectileInstance.GetComponent<ProjectileImpactExplosion>();
            //Debug.Log("pie: " + pie);
            //pd = projectileInstance.GetComponent<ProjectileDamage>();   
            //pd.damageType = (DamageType)Moonstorm.Starstorm2.DamageTypes.NetOnHit.netDamageType;
            //Debug.Log("damage type: " + pd.damageType);

            var damageAPIComponent = projectileInstance.AddComponent<ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(NetOnHit.netDamageType);

            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    crit = RollCrit(),
                    damage = damageCoefficient * damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 0f,
                    owner = gameObject,
                    position = aimRay.origin,
                    procChainMask = default(ProcChainMask),
                    projectilePrefab = projectileInstance,
                    rotation = Quaternion.LookRotation(aimRay.direction),
                    useSpeedOverride = false
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }    
        }
    }
}
