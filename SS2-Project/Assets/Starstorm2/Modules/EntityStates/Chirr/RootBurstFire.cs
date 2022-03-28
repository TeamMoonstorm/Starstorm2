using Moonstorm;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Chirr
{
    public class RootBurstFire : BaseState
    {
        [TokenModifier("SS2_CHIRR_ROOT_DESCRIPTION", StatTypes.Percentage, 0)]
        public static float damageCoefficient;
        public static float baseDuration;
        public static float minSpread;
        public static float maxSpread;
        public static float pitchRange;
        public static float yawSpread;
        public static float projSpeed;
        public static float castRange;
        public static int minCharge;
        public static int maxCharge;
        public static GameObject muzzleflastEffectPrefab;
        public static GameObject projectilePrefab;

        public float charge;

        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            PlayFireAnimation();
            if (muzzleflastEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleflastEffectPrefab, base.gameObject, "MuzzleFace", false);
            }
            FireBurst();
        }

        public void PlayFireAnimation()
        {
            // fire animation goes here lmao
        }

        public void FireBurst()
        {
            if (base.isAuthority)
            {
                if (projectilePrefab != null)
                {
                    Ray aimRay = base.GetAimRay();
                    aimRay.origin = aimRay.GetPoint(3f);
                    float magnitude = projSpeed;
                    RaycastHit raycastHit;

                    var vector = (Util.CharacterRaycast(base.gameObject, aimRay, out raycastHit, castRange, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore) ? raycastHit.point : aimRay.GetPoint(castRange)) - aimRay.origin;
                    var vector2 = new Vector2(vector.x, vector.z);
                    float magnitude2 = vector2.magnitude;
                    float y = Trajectory.CalculateInitialYSpeed(magnitude2 / magnitude, vector.y);
                    Vector3 a = new Vector3(vector2.x / magnitude2 * magnitude, y, vector2.y / magnitude2 * magnitude);
                    magnitude = a.magnitude;
                    aimRay.direction = a / magnitude;

                    int num = Mathf.RoundToInt(Util.Remap(charge, 0f, 1f, minCharge, maxCharge));
                    if (base.isAuthority)
                    {
                        for (int i = 0; i < num; i++)
                        {
                            FireSingle(aimRay, pitchRange, (num / 2f - (float)i) * yawSpread, magnitude);
                        }
                    }
                }
            }
        }

        public void FireSingle(Ray aimRay, float bonusPitch, float bonusYaw, float speed)
        {
            float randomPitch = Random.Range(-bonusPitch, bonusPitch);
            Vector3 forward = Util.ApplySpread(aimRay.direction, minSpread, maxSpread, 1f, 1f, bonusYaw, randomPitch);
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = projectilePrefab,
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(forward),
                owner = base.gameObject,
                damage = damageStat * damageCoefficient,
                force = 0f,
                crit = base.RollCrit(),
                speedOverride = speed
            };

            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
