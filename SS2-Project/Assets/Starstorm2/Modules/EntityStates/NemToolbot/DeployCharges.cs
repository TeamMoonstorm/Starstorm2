using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.NemToolbot
{
    /// <summary>
    /// Utility skill shared between deployed and ball forms.
    /// Drops 4 charge projectiles at cardinal directions around the character.
    /// Each charge detonates after a delay (handled by the projectile prefab's
    /// ProjectileImpactExplosion with lifetime).
    /// </summary>
    public class DeployCharges : BaseSkillState
    {
        public static float damageCoefficient = 4f;
        public static float baseDuration = 0.5f;
        public static float chargeSpawnRadius = 2f;
        public static float force = 500f;

        public static string soundString = "";
        public static GameObject muzzleFlashPrefab;
        public static GameObject chargeProjectilePrefab;

        private float duration;

        // Cardinal offsets relative to the character's forward direction
        private static readonly Vector3[] cardinalOffsets = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            Util.PlaySound(soundString, gameObject);
            if (muzzleFlashPrefab != null)
            {
                EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, "Muzzle", transmit: false);
            }

            PlayCrossfade("Gesture, Override", "DeployCharges", "DeployCharges.playbackRate", duration, 0.05f);

            if (isAuthority)
            {
                FireCharges();
            }
        }

        private void FireCharges()
        {
            if (chargeProjectilePrefab == null)
            {
                Debug.LogError("NemToolbot DeployCharges: chargeProjectilePrefab is null.");
                return;
            }

            Vector3 footPosition = characterBody.footPosition;
            Quaternion characterRotation = Quaternion.LookRotation(characterDirection != null ? characterDirection.forward : transform.forward, Vector3.up);

            DamageTypeCombo damageType = DamageType.Generic;
            damageType.damageSource = DamageSource.Utility;

            for (int i = 0; i < cardinalOffsets.Length; i++)
            {
                Vector3 worldOffset = characterRotation * (cardinalOffsets[i] * chargeSpawnRadius);
                Vector3 spawnPosition = footPosition + worldOffset;

                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.projectilePrefab = chargeProjectilePrefab;
                fireProjectileInfo.position = spawnPosition;
                fireProjectileInfo.rotation = Quaternion.identity;
                fireProjectileInfo.owner = gameObject;
                fireProjectileInfo.damage = damageStat * damageCoefficient;
                fireProjectileInfo.force = force;
                fireProjectileInfo.crit = RollCrit();
                fireProjectileInfo.damageTypeOverride = damageType;

                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
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
