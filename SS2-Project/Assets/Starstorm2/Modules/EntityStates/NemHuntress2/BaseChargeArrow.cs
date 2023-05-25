using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemHuntress2
{
    public class BaseChargeArrow : BaseSkillState
    {
        [SerializeField]
        public float baseChargeDuration;
        private float chargeDuration;

        [SerializeField]
        public float maxDamageCoefficient;
        [SerializeField]
        public float minDamageCoefficient;
        [SerializeField]
        public float procCoefficient;
        [SerializeField]
        public float minRecoil;
        [SerializeField]
        public float maxRecoil;
        [SerializeField]
        public float minProjectileSpeed;
        [SerializeField]
        public float maxProjectileSpeed;
        [SerializeField]
        public float fireDuration;

        [SerializeField]
        public NetworkSoundEventDef sound;
        private bool hasPlayedSound = false;

        [SerializeField]
        public GameObject projectilePrefab;

        //public EntityState releaseState;

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration / attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            if (charge > 1f)
            {
                charge = 1f;
                if (sound != null && !hasPlayedSound)
                {
                    hasPlayedSound = true;
                    EffectManager.SimpleSoundEffect(sound.index, transform.position, true);
                }
            }

            characterBody.SetSpreadBloom(Util.Remap(charge, 0f, 1f, 0f, 1f), true);

            StartAimMode();

            if (isAuthority && !IsKeyDownAuthority() && fixedAge >= 0.1f)
            {
                BaseFireArrow nextState = new BaseFireArrow(); //create & modify firing state
                nextState.charge = charge;
                nextState.projectilePrefab = projectilePrefab;
                nextState.maxDamageCoefficient = maxDamageCoefficient;
                nextState.minDamageCoefficient = minDamageCoefficient;
                nextState.procCoefficient = procCoefficient;
                nextState.minRecoil = minRecoil;
                nextState.maxRecoil = maxRecoil;
                nextState.minProjectileSpeed = minProjectileSpeed;
                nextState.maxProjectileSpeed = maxProjectileSpeed;
                nextState.baseDuration = fireDuration;
                outer.SetNextState(nextState); //set to fire with all our modifications in place 
            }    
        }

        protected float CalcCharge()
        {
            return Mathf.Clamp01(fixedAge / chargeDuration);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
