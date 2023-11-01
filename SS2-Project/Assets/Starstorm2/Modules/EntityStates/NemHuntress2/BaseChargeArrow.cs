using RoR2;
using RoR2.Skills;
using RoR2.UI;
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
        public float maxSpreadAngle;

        //[SerializeField]
        //public NetworkSoundEventDef sound;
        private bool hasPlayedSound = false;

        /*[SerializeField]
        public GameObject projectilePrefab;*/

        private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

        private float baseSpreadRate;

        //public EntityState releaseState;

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration / attackSpeedStat;
            Util.PlayAttackSpeedSound("NemHuntressChargeBow", gameObject, attackSpeedStat);
            crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(characterBody, characterBody.defaultCrosshairPrefab, CrosshairUtils.OverridePriority.PrioritySkill);
            baseSpreadRate = characterBody.spreadBloomDecayTime;
            characterBody.spreadBloomDecayTime = 50f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            if (charge >= 1f)
            {
                charge = 1f;
                if (!hasPlayedSound)
                {
                    hasPlayedSound = true;
                    Util.PlaySound("NemHuntressBowMax", gameObject);
                    //EffectManager.SimpleSoundEffect(sound.index, transform.position, true);
                }
            }

            characterBody.SetSpreadBloom(Util.Remap(charge, 0f, 1f, 0f, 1f), true);

            StartAimMode();

            if (!IsKeyDownAuthority() && fixedAge >= 0.1f)
            {
                BaseFireArrow nextState = new BaseFireArrow(); //create & modify firing state
                nextState.charge = charge;
                //nextState.projectilePrefab = projectilePrefab;
                nextState.maxDamageCoefficient = maxDamageCoefficient;
                nextState.minDamageCoefficient = minDamageCoefficient;
                nextState.procCoefficient = procCoefficient;
                nextState.minRecoil = minRecoil;
                nextState.maxRecoil = maxRecoil;
                nextState.minProjectileSpeed = minProjectileSpeed;
                nextState.maxProjectileSpeed = maxProjectileSpeed;
                nextState.baseDuration = fireDuration;
                nextState.maxSpreadAngle = maxSpreadAngle;
                outer.SetNextState(nextState); //set to fire with all our modifications in place 
            }   //in retrospect i think none of this makes any sense..
        }   //but it works lol 

        protected float CalcCharge()
        {
            return Mathf.Clamp01(fixedAge / chargeDuration);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void OnExit()
        {
            CrosshairUtils.OverrideRequest overrideRequest = crosshairOverrideRequest;
            if (overrideRequest != null)
            {
                overrideRequest.Dispose();
            }
            base.OnExit();
            characterBody.spreadBloomDecayTime = baseSpreadRate;
        }
    }
}
