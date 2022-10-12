using RoR2;
using Moonstorm.Starstorm2;
using UnityEngine;
using System.Collections.Generic;

namespace EntityStates.Nemmando
{
    public class ScepterBarrageCharge : BaseSkillState
    {
        public static float baseChargeDuration = 1.5f;
        public static GameObject chargeEffect;
        public static GameObject scepterChargeEffect;

        private float chargeDuration;
        private ChildLocator childLocator;
        private Animator animator;
        private Transform modelBaseTransform;
        private uint chargePlayID;
        private bool hasFinishedCharging;
        
        private GameObject chargeEffectInstance;
        int chargeState = 0;
        private List<GameObject> chargeEffectInstances;
        

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration;// / attackSpeedStat;
            childLocator = GetModelChildLocator();
            modelBaseTransform = GetModelBaseTransform();
            animator = GetModelAnimator();
            hasFinishedCharging = false;
            //SS2Log.Debug("test!!!!!!!");
            chargePlayID = Util.PlaySound("NemmandoSubmissionCharge", gameObject);
            chargeState = 0;
            chargeEffectInstances = new List<GameObject>();
            //chargeEffect = childLocator.FindChild("GunChargeEffect").gameObject;
            //if ((bool)chargeEffect)
            //{
            //    var muzzle = childLocator.FindChild("GunChargeEffect");
            //    chargeEffectInstance = Object.Instantiate(chargeEffect, muzzle);
            //}
            //if (chargeEffect) chargeEffect.SetActive(true);
            if ((bool)chargeEffect)
            {
                var muzzle = childLocator.FindChild("GunChargeEffect");
                chargeEffectInstance = Object.Instantiate(chargeEffect, muzzle);
                chargeEffectInstances.Add(chargeEffectInstance);
                //chargeEffectInstances[chargeState] = Object.Instantiate(chargeEffect, muzzle);
                
                
                chargeState++;
            }
            //if (cameraTargetParams) cameraTargetParams.aimMode = CameraTargetParams.AimType.Aura;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            if (charge >= (.3 * (chargeState - 1)) && chargeState == chargeEffectInstances.Count && charge < .5)
            {
                //SS2Log.Debug("charge: " + charge + " | state: " + chargeState + " | chargeEffectInstances Length: " + chargeEffectInstances.Count);
                if ((bool)scepterChargeEffect)
                {
                    var muzzle = childLocator.FindChild("GunChargeEffect");
                    chargeEffectInstance = Object.Instantiate(scepterChargeEffect, muzzle);
                    chargeEffectInstances.Add(chargeEffectInstance);
                    //chargeEffectInstances[chargeState] = Object.Instantiate(chargeEffect, muzzle);
                }
                chargeState++;
            }
            //else if (charge >= .4 && chargeState == 1)
            //{
            //    if ((bool)chargeEffect)
            //    {
            //        var muzzle = childLocator.FindChild("GunChargeEffect");
            //        chargeEffectInstances[chargeState] = Object.Instantiate(chargeEffect, muzzle);
            //    }
            //    chargeState++;
            //}
            
            if (charge >= 1f)
            {
                if (!hasFinishedCharging)
                {
                    hasFinishedCharging = true;
                    Util.PlaySound("NemmandoSubmissionReady", gameObject);
                }
            }

            if (isAuthority && ((!IsKeyDownAuthority() && fixedAge >= 0.1f)))
            {
                ScepterBarrageFire nextState = new ScepterBarrageFire();
                nextState.charge = charge;
                outer.SetNextState(nextState);
            }
        }

        protected float CalcCharge()
        {
            return Mathf.Clamp01(fixedAge / chargeDuration);
        }

        public override void OnExit()
        {
            base.OnExit();
            AkSoundEngine.StopPlayingID(chargePlayID);

            if (chargeEffectInstance)
                Destroy(chargeEffectInstance);
            if(chargeEffectInstances.Count > 0)
            {
                for(int i = 0; i < chargeEffectInstances.Count; i++)
                {
                    Destroy(chargeEffectInstances[i]);
                }
            }
            //if (chargeEffect) chargeEffect.SetActive(false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}