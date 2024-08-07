using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2;
using RoR2.Projectile;
using R2API.ScriptableObjects;

namespace EntityStates.Nuke.Weapon
{
    public class FireSludge : BaseNukeFireState
    {
        public static GameObject projectilePrefab;
        public static SerializableDamageColor damageColor;
        public static float force;
        public static float baseSpeed;
        public static float baseDuration;
        [HideInInspector]
        public static GameObject muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoDiseaseImpactEffect.prefab").WaitForCompletion();

        public string chosenMuzzleString;
        public Transform muzzleTransform;

        private float duration;
        private Ray aimRay;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            aimRay = GetAimRay();
            if (muzzleFlashPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, chosenMuzzleString, true);
            }

            GameObject earlyTarget = null;
            if (Util.CharacterRaycast(gameObject, aimRay, out var hitInfo, 1024f, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
            {
                Debug.Log(hitInfo.collider);
                if (hitInfo.collider)
                {
                    earlyTarget = hitInfo.collider.gameObject;
                }
            }

            FireProjectileInfo projectileInfo = new FireProjectileInfo
            {
                crit = RollCrit(),
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                owner = gameObject,
                projectilePrefab = projectilePrefab,
                damage = damageStat * charge,
                damageColorIndex = damageColor.DamageColorIndex,
                force = force,
                useSpeedOverride = true,
                speedOverride = baseSpeed * charge,
            };
            if (earlyTarget)
            {
                projectileInfo.target = earlyTarget;
            }
            ProjectileManager.instance.FireProjectile(projectileInfo);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration)
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
