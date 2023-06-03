using Moonstorm.Starstorm2;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.MULE
{
    public class MULEChargePunch : BaseSkillState
    {
        public static float baseChargeDuration = 0.5f;
        public static float minSpinCount;
        public static float maxSpinCount;
        public static float duration = 0.5f;
        private float chargeDuration;
        private Animator animator;
        private ChildLocator childLocator;
        private string spinFXmuzzle = "SpinFXArm";
        private GameObject spinFX;

        public float spinCount = 0f;

        public override void OnEnter()
        {
            base.OnEnter();
            spinCount++;
            chargeDuration = baseChargeDuration / attackSpeedStat;
            animator = GetModelAnimator();
            childLocator = GetModelChildLocator();
            spinFX = childLocator.FindChild(spinFXmuzzle).gameObject;
            if (spinFX)
                spinFX.SetActive(true);
            characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 0);
            PlayCrossfade("Gesture, Override", "ChargePunch", "Secondary.playbackRate", duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            characterBody.SetSpreadBloom(Util.Remap(charge, 0f, 1f, 0f, 3f), true);

            StartAimMode();

            if (isAuthority && (charge >= 1f))
            {
                if (inputBank.skill2.down && spinCount < maxSpinCount)
                {
                    //Charge More
                    Debug.Log("Charge Another " + spinCount);
                    MULEChargePunch nextState = new MULEChargePunch();
                    nextState.spinCount = spinCount;
                    outer.SetNextState(nextState);
                }
                else
                {
                    //Punch!
                    Debug.Log("Punching " + spinCount);
                    if (spinFX)
                        spinFX.SetActive(false);
                    MULEStunPunch nextState = new MULEStunPunch();
                    nextState.spinCount = spinCount;
                    outer.SetNextState(nextState);
                }
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

        public override void OnExit()
        {
            Debug.Log("Exiting!");
            if (spinFX)
                spinFX.SetActive(false);
            base.OnExit();
        }
    }
}
