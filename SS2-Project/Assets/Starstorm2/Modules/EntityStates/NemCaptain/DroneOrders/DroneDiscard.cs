using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm.Starstorm2.Components;
using RoR2.Skills;
using RoR2.Projectile;
using RoR2;

namespace EntityStates.NemCaptain.Weapon
{
    public class DroneDiscard : BaseSkillState
    {
        public static GameObject projectilePrefab;
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float projectileSpeed;
        public static float baseDuration;
        public static float recoil;
        public static string muzzleString;
        private NemCaptainController ncc;

        private float duration;
        private float shotDur;
        private int shotsToFire;
        private int timesFired;
        private float timer;

        public override void OnEnter()
        {
            base.OnEnter();

            ncc = GetComponent<NemCaptainController>();

            SkillDef[] handSkills = new SkillDef[] { ncc.hand1.skillDef, ncc.hand2.skillDef, ncc.hand3.skillDef, ncc.hand4.skillDef };

            shotsToFire = 0;
            foreach(SkillDef skill in handSkills)
            {
                if (skill != ncc.nullSkill)
                {
                    shotsToFire++;
                }
            }
            duration = (baseDuration * shotsToFire) / attackSpeedStat;
            shotDur = baseDuration / attackSpeedStat;
            characterBody.SetAimTimer(duration * 1.2f);
            timesFired = 0;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            timer += Time.fixedDeltaTime;
            if (timer >= shotDur && timesFired < shotsToFire && isAuthority)
            {
                timer = 0f;
                timesFired++;
            }
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public virtual void FireDrone()
        {
            if (isAuthority)
            {
                float damage = damageCoefficient * damageStat;
                AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
                characterBody.AddSpreadBloom(0.33f * recoil);
                Ray aimRay = GetAimRay();

                Debug.Log("fired shot :D");
                //ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), gameObject, damage, 0f, RollCrit(), DamageColorIndex.Default, null, projectileSpeed);
            }
        }

        public override void OnExit()
        {
            ncc.DiscardCardsAndReplace();
            base.OnExit();
        }
    }
}
