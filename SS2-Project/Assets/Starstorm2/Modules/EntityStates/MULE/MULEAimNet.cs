using UnityEngine;
using RoR2;
using RoR2.Projectile;
using static R2API.DamageAPI;

namespace EntityStates.MULE
{
#if DEBUG
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
            duration = baseDuration / attackSpeedStat;
            Util.PlaySound(enterSoundString, gameObject);
            PlayCrossfade("Gesture, Override", "ShootNet", 0.05f);

            projectileInstance = projectilePrefab;
            Debug.Log("projectile instance: " + projectileInstance);

            var damageAPIComponent = projectileInstance.AddComponent<ModdedDamageTypeHolderComponent>();
            damageAPIComponent.Add(SS2.Survivors.MULE.NetDamageType);

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
#endif
}
