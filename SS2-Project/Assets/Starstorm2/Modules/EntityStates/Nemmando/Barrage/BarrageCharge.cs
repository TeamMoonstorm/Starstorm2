using RoR2;
using UnityEngine;

namespace EntityStates.Nemmando
{
    public class BarrageCharge : BaseSkillState
    {
        public static float baseChargeDuration;
        public static GameObject chargeEffect;

        private float chargeDuration;
        private ChildLocator childLocator;
        private Animator animator;
        private Transform modelBaseTransform;
        private uint chargePlayID;
        private bool hasFinishedCharging;
        private GameObject chargeEffectInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            chargeDuration = baseChargeDuration;// / attackSpeedStat;
            childLocator = GetModelChildLocator();
            modelBaseTransform = GetModelBaseTransform();
            animator = GetModelAnimator();
            hasFinishedCharging = false;

            chargePlayID = Util.PlaySound("NemmandoSubmissionCharge", gameObject);

            if ((bool)chargeEffect)
            {
                var muzzle = childLocator.FindChild("GunChargeEffect");
                chargeEffectInstance = Object.Instantiate(chargeEffect, muzzle);
            }
            /*chargeEffect = childLocator.FindChild("GunChargeEffect").gameObject;

            if (chargeEffect)
                chargeEffect.SetActive(true);*/
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            if (charge >= 1f)
            {
                if (!hasFinishedCharging)
                {
                    hasFinishedCharging = true;
                    Util.PlaySound("NemmandoSubmissionReady", gameObject);
                    //PlayCrossfade("Gesture, Override", "RaiseGun", "FireGun.playbackRate", .75f, .25f);
                }
            }

            if (isAuthority && !IsKeyDownAuthority() && fixedAge >= 0.1f)
            {
                BarrageFire nextState = new BarrageFire();
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
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}