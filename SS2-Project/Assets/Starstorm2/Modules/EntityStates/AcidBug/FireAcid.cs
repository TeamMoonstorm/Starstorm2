using System;
using UnityEngine;
using RoR2;
using RoR2.Projectile;

namespace EntityStates.AcidBug
{
    public class ChargeAcid : BaseSkillState
    {
        public static GameObject effectPrefab;
        private static float baseDuration = 0.5f;
        private static string enterSoundString = "ChirrFireSpitBomb";

        private float duration;
        private GameObject chargeEffect;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            
            PlayAnimation("FullBody, Override", "ChargeAcid", "ChargeAcid.playbackRate", duration);
            Util.PlaySound(enterSoundString, gameObject);
            Transform muzzle = FindModelChild("Muzzle");
            if (muzzle)
            {
                chargeEffect = GameObject.Instantiate<GameObject>(effectPrefab, muzzle.position, muzzle.rotation);
                chargeEffect.transform.parent = muzzle;
                ScaleParticleSystemDuration scaleParticleSystemDuration = chargeEffect.GetComponent<ScaleParticleSystemDuration>();
                if (scaleParticleSystemDuration)
                {
                    scaleParticleSystemDuration.newDuration = this.duration;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextState(new FireAcid());
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (chargeEffect)
            {
                Destroy(chargeEffect);
            }
        }
    }

    public class FireAcid : BaseState
    {
        public static GameObject projectilePrefab;
        public static GameObject muzzleEffectPrefab;
        private static float minSpeed = 80f;
        private static float maxSpeed = 100f;
        private static float minSpread = 2f;
        private static float maxSpread = 15f;
        private static float spreadYaw = 2f;
        private static float spreadPitch = 1f;
        private static int projectileCount = 3;
        private static string attackSoundString = "ChirrFireSpitBomb";
        private string muzzleName = "Muzzle";
        private static float baseDuration = 0.5f;
        private static float damageCoefficient = 1f;
        private static float force = 100f;

        private static float selfAwayForce = 11f;
        private static float selfUpForce = 11f;

        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            StartAimMode();
            PlayAnimation("FullBody, Override", "FireAcid", "FireAcid.playbackRate", this.duration);
            Util.PlaySound(attackSoundString, gameObject);
            EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleName, false);

            Fire();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void Fire()
        {
            Ray aimRay = GetAimRay();
            Vector3 direction = aimRay.direction;
            if (isAuthority)
            {
                Vector3 awayForce = -1f * direction * selfAwayForce;
                awayForce += Vector3.up * selfUpForce;
                rigidbody.AddForce(awayForce, ForceMode.Force);

                bool crit = RollCrit();
                for (int i = 0; i < projectileCount; i++)
                {
                    Vector3 forward = Util.ApplySpread(direction, minSpread, maxSpread, spreadYaw, spreadPitch);
                    float speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
                    FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                    {
                        projectilePrefab = projectilePrefab,
                        position = aimRay.origin,
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
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
