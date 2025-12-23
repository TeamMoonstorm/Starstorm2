using System;
using UnityEngine;
using RoR2;
using RoR2.Projectile;

namespace EntityStates.AcidBug
{
    public class FireAcid : BaseState
    {
        public static GameObject chargeEffectPrefab;
        public static GameObject projectilePrefab;
        public static GameObject muzzleEffectPrefab;

        private static float minSpeed = 40f;
        private static float maxSpeed = 70f;
        private static float minSpread = 2f;
        private static float maxSpread = 10f;
        private static float spreadYaw = 2f;
        private static float spreadPitch = 1f;
        private static int projectileCount = 3;
        private static string enterSoundString = "ChirrFireSpitBomb";
        private static string attackSoundString = "ChirrFireSpitBomb";
        private string muzzleName = "Muzzle";
        private static float baseDuration = 1.25f;
        private static float fireTime = 0.55f;
        private static float damageCoefficient = 1f;
        private static float force = 100f;

        private static float selfAwayForce = 11f;
        private static float selfUpForce = 11f;

        private float duration;
        private float chargeDuration => duration * fireTime;
        private GameObject chargeEffect;
        private bool hasFired;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            rigidbodyMotor.moveVector = Vector3.zero;
            Util.PlaySound(enterSoundString, gameObject);
            Transform muzzle = FindModelChild("Muzzle");
            if (muzzle)
            {
                chargeEffect = GameObject.Instantiate<GameObject>(chargeEffectPrefab, muzzle.position, muzzle.rotation);
                chargeEffect.transform.parent = muzzle;
                ScaleParticleSystemDuration scaleParticleSystemDuration = chargeEffectPrefab.GetComponent<ScaleParticleSystemDuration>();
                if (scaleParticleSystemDuration)
                {
                    scaleParticleSystemDuration.newDuration = chargeDuration;
                }

            }
            StartAimMode();
            PlayAnimation("FullBody, Override", "FireAcid", "FireAcid.playbackRate", this.duration);
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hasFired && fixedAge >= chargeDuration)
            {
                Fire();
            }
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void Fire()
        {
            hasFired = true;

            Util.PlaySound(attackSoundString, gameObject);
            EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleName, false);
            if (chargeEffect)
            {
                Destroy(chargeEffect);
            }

            Ray aimRay = GetAimRay();
            Vector3 direction = aimRay.direction;
            if (isAuthority)
            {
                Vector3 awayForce = -1f * direction * selfAwayForce;
                awayForce += Vector3.up * selfUpForce;
                rigidbody.AddForce(awayForce, ForceMode.Force);
                var muzzleTransform = FindModelChild("Muzzle");
                bool crit = RollCrit();
                for (int i = 0; i < projectileCount; i++)
                {
                    Vector3 forward = Util.ApplySpread(direction, minSpread, maxSpread, spreadYaw, spreadPitch);
                    float speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        projectilePrefab = projectilePrefab,
                        position = muzzleTransform ? muzzleTransform.position : aimRay.origin,
                        rotation = Util.QuaternionSafeLookRotation(forward),
                        owner = gameObject,
                        damage = damageStat * damageCoefficient,
                        force = force,
                        crit = crit,
                        speedOverride = speed,
                    };
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
        }
        public override void OnExit()
        {
            if (chargeEffect)
            {
                Destroy(chargeEffect);
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
