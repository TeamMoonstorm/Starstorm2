using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.LampBoss
{
    public class FireOrbs : BaseSkillState
    {
        public static float damageCoefficient;
        public static float recoil;
        public static float projectileSpeed;
        public static float baseDuration;
        public static string mecanimPerameter;
        public static float baseTimeBetweenShots;
        public static GameObject projectilePrefab;
        public static GameObject blueProjectilePrefab;
        public static string muzzleString;
        public static float maxOrbCount;
        private float orbCount = 0;

        private float timer;
        private float duration;
        private float timeBetweenShots;
        private Transform muzzle;
        private Animator animator;
        private Ray aimRay;
        private GameObject projectile;

        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.SetAimTimer(duration * 1.5f);
            duration = baseDuration / attackSpeedStat;
            timeBetweenShots = baseTimeBetweenShots / attackSpeedStat;
            muzzle = GetModelChildLocator().FindChild(muzzleString);
            animator = GetModelAnimator();
            aimRay = GetAimRay();

            bool isBlue = GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[characterBody.skinIndex].nameToken == "SS2_SKIN_LAMP_BLUE";
            projectile = isBlue ? blueProjectilePrefab : projectilePrefab;

            Util.PlayAttackSpeedSound("WayfarerAttack", gameObject, attackSpeedStat);

            PlayCrossfade("FullBody, Override", "HoldLamp", "Primary.playbackRate", duration, 0.05f);
        }

        public virtual void FireProjectile()
        {
            aimRay = GetAimRay();
            float damage = damageCoefficient * damageStat;
            AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
            characterBody.AddSpreadBloom(0.33f * recoil);

            Vector3 angle = Quaternion.Euler(Random.Range(-18.5f, 18.5f), Random.Range(-2.5f, 22.5f), 0f) * aimRay.direction;

            //Util.PlayAttackSpeedSound("LampBullet", gameObject, characterBody.attackSpeed);

            ProjectileManager.instance.FireProjectile(
                projectile,
                muzzle.position,
                Util.QuaternionSafeLookRotation(angle), 
                gameObject, 
                damage, 
                20f, 
                RollCrit(), 
                DamageColorIndex.Default, 
                null, 
                projectileSpeed);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            timer += Time.fixedDeltaTime;

            if (animator.GetFloat("RaiseLamp") >= 0.5f && orbCount < maxOrbCount)
            {
                if (timer >= baseTimeBetweenShots / attackSpeedStat)
                {
                    orbCount++;
                    timer = 0;
                    if (isAuthority)
                        FireProjectile();
                    //Debug.Log("firing projectile");
                }
            }

            if (fixedAge >= duration && isAuthority)
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
