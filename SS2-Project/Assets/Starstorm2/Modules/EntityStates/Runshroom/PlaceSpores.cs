using Moonstorm;
using Moonstorm.Starstorm2;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Runshroom
{
    public class PlaceSpores : BaseSkillState
    {
        private GameObject projectilePrefab;
        private bool hasFired;
        private bool hasWaited;
        public static float baseDuration;
        public static float damageCoefficient = 1f;
        public static float projectileVelocity = 40f;
        public static GameObject chargeEffectPrefab;
        private float duration;
        private Animator animator;
        private ChildLocator childLocator;

        private float originalMoveSpeed;

        public override void OnEnter()
        {
            base.OnEnter();
            projectilePrefab = MiniMushroom.SporeGrenade.projectilePrefab;
            originalMoveSpeed = characterBody.moveSpeed;
            characterBody.moveSpeed = 0f;
            PlayCrossfade("Body", "ToIdle", 0.05f);
            duration = baseDuration / attackSpeedStat;
            childLocator = GetModelChildLocator();
            //characterBody.SetAimTimer(duration * 1.5f);
            hasFired = false;
            hasWaited = false;
            animator = GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.moveSpeed = 0f;

            if (fixedAge >= duration * 0.2f)
            {
                if (!hasWaited)
                {
                    hasWaited = true;
                    GameObject chargeEffectPrefabInstance = Object.Instantiate(chargeEffectPrefab);
                    chargeEffectPrefabInstance.transform.SetParent(childLocator.FindChild("TippyTop"));
                    //chargeEffectPrefabInstance.transform.position = Vector3.zero;
                    chargeEffectPrefabInstance.transform.position = childLocator.FindChild("TippyTop").position;
                    PlayCrossfade("Body", "Attack", "Primary.playbackRate", duration, 0.1f);
                }
            }

            if (animator.GetFloat("Attack") > 0.5f && !hasFired)
            {
                hasFired = true;
                if (isAuthority)
                    FireProjectile();
            }

            if (fixedAge >= duration * 1.2f && hasFired)
                outer.SetNextStateToMain();
        }

        public void FireProjectile()
        {
            float damage = damageCoefficient * damageStat;
            Ray aimRay = GetAimRay();
            ProjectileManager.instance.FireProjectile(
                projectilePrefab,
                aimRay.origin,
                Util.QuaternionSafeLookRotation(Vector3.down),
                gameObject,
                damage,
                0f,
                RollCrit(),
                DamageColorIndex.Default,
                null,
                projectileVelocity);
        }

        public override void OnExit()
        {
            base.OnExit();
            characterBody.moveSpeed = originalMoveSpeed;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}