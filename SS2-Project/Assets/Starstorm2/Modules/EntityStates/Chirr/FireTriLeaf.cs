using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using RoR2.Projectile;
namespace EntityStates.Chirr
{
    
    public class FireTriLeaf : BaseSkillState
    {
        public static GameObject projectilePrefab;
        public string soundString;
        public GameObject muzzleEffectPrefab = null;

        public static float baseDuration = 0.8f;
        public static float fireTime = 0.3f;
        public static int numShots = 3;
        public static float bloom = 1f;
        public static float recoilAmplitude = 0.1f;

        public static float damageCoefficient;
        public static float procCoefficient;
        public static float force;

        public static float minSpread = 0f;
        public static float maxSpread = 1f;

        private float duration;
        private int shotsFired;
        private float fireInterval;
        private float fireStopwatch;

        private bool isCrit;

        public static float autoAimRadius = 2.5f;
        public static float autoAimDistance = 50f;
        public override void OnEnter()
        {
            base.OnEnter();

            this.duration = baseDuration / this.attackSpeedStat;
            this.fireInterval = duration * fireTime / numShots;

            this.isCrit = base.RollCrit();
            //Util.PlaySound();
            //base.PlayAnimation();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.fireStopwatch -= Time.fixedDeltaTime;

            if(this.fireStopwatch <= 0 && this.shotsFired < numShots)
            {
                this.fireStopwatch += this.fireInterval;
                this.Fire();
            }

            if(base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            this.shotsFired++;
            //Util.PlaySound()
            //Animation??
            //EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, this.muzzleName, true);
            AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
            base.characterBody.AddSpreadBloom(bloom);

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                aimRay.direction = Util.ApplySpread(aimRay.direction, minSpread, maxSpread, 1f, 1f);
                ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, 
                    Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, 
                    this.damageStat * damageCoefficient, force, this.isCrit);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
