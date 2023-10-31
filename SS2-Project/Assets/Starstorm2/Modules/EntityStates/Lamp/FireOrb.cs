using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Lamp
{
    public class FireOrb : BaseSkillState
    {
        public static float damageCoefficient;
        public static float recoil;
        public static float projectileSpeed;
        public static float baseDuration;
        public static string mecanimPeramater;
        public static GameObject projectilePrefab;
        public static GameObject blueProjectilePrefab;
        public static string muzzleString;

        private float duration;
        private bool hasFired;
        private Transform muzzle;
        private Animator animator;

        private float originalMoveSpeed;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.SetAimTimer(duration * 1.5f);
            duration = baseDuration / attackSpeedStat;
            muzzle = GetModelChildLocator().FindChild(muzzleString);
            animator = GetModelAnimator();
            hasFired = false;

            originalMoveSpeed = characterBody.moveSpeed;

            PlayCrossfade("Body", "Attack", "Primary.playbackRate", duration, 0.05f);
        }

        public virtual void FireProjectile()
        {
                float damage = damageCoefficient * damageStat;
                AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
                characterBody.AddSpreadBloom(0.33f * recoil);
                Ray aimRay = GetAimRay();

            bool isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";
            GameObject projectile = isBlue ? blueProjectilePrefab : projectilePrefab;

            Util.PlayAttackSpeedSound("LampBullet", gameObject, characterBody.attackSpeed);

            ProjectileManager.instance.FireProjectile(
                    projectile,
                    muzzle.position,
                    Util.QuaternionSafeLookRotation(aimRay.direction),
                    gameObject,
                    damage,
                    30f,
                    RollCrit(),
                    DamageColorIndex.Default,
                    null,
                    projectileSpeed);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.moveSpeed = originalMoveSpeed * 0.5f;

            if (animator.GetFloat("Fire") >= 0.5f && !hasFired)
            {
                hasFired = true;
                FireProjectile();
            }

            if (fixedAge >= duration && isAuthority)
                outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
            characterBody.moveSpeed = originalMoveSpeed;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
