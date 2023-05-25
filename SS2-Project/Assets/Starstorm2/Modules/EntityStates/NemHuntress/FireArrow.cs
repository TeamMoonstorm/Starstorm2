using Moonstorm;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.NemHuntress
{
    public class FireArrow : BaseSkillState
    {
        public float charge;

        public static float maxDamageCoefficient;
        public static float minDamageCoeffficient;
        public static float procCoefficient;
        public static float maxRecoil;
        public static float minRecoil;
        public static float maxProjectileSpeed;
        public static float minProjectileSpeed;
        public static float baseDuration;
        public static GameObject projectilePrefab;

        private float damageCoefficient;
        private float recoil;
        private float projectileSpeed;
        private float duration;
        private float fireDuration;
        private bool hasFired;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();
            hasFired = false;
            duration = baseDuration / attackSpeedStat;
            damageCoefficient = Util.Remap(charge, 0f, 1f, minDamageCoeffficient, maxDamageCoefficient);
            recoil = Util.Remap(charge, 0f, 1f, minRecoil, maxRecoil);
            projectileSpeed = Util.Remap(charge, 0f, 1f, minProjectileSpeed, maxProjectileSpeed);
            fireDuration = 0.4f * duration;

            string fireAnim = charge > 0.6f ? "Secondary3(Strong)" : "Secondary3(Weak)";

            bool moving = animator.GetBool("isMoving");
            bool grounded = animator.GetBool("isGrounded");

            if (!moving && grounded)
            {
                //PlayCrossfade("FullBody, Override", fireAnim, "Secondary.playbackRate", duration, 0.05f);
            }

            //PlayCrossfade("Gesture, Override", fireAnim, "Secondary.playbackRate", duration, 0.05f);

            //Util.PlaySound("NemmandoFireBeam2", gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();

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