using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;
using RoR2.Orbs;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.Chirr
{

    public class Befriend : BaseSkillState
    {
        public string soundString;
        public GameObject muzzleEffectPrefab = null;

        public static float baseDuration = 0.5f;
        public static float fireTime = 0.0f;
        public static float bloom = 1f;
        public static float recoilAmplitude = 0.1f;

        public static float damageCoefficient;
        public static float procCoefficient;
        public static float force;

        private bool hasFired;
        private float duration;
        private ChirrFriendTracker tracker;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            this.tracker = base.GetComponent<ChirrFriendTracker>();
            //Util.PlaySound();
            //base.PlayAnimation();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!this.hasFired && base.fixedAge >= fireTime * this.duration)
            {
                this.Fire();
                this.hasFired = true;
            }

            if (base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire()
        {
            //Util.PlaySound()
            //Animation??
            //EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, this.muzzleName, true);
            AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
            base.characterBody.AddSpreadBloom(bloom);

            if(NetworkServer.active)
            {
                OrbManager.instance.AddOrb(new ChirrFriendOrb { 
                    attacker = base.gameObject,
                    target = this.tracker.GetTrackingTarget(),
                    tracker = this.tracker, 
                    origin = base.characterBody.corePosition, // muzzle
                    damageType = DamageType.Stun1s, 
                    procCoefficient = 0 
                });
            }

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
