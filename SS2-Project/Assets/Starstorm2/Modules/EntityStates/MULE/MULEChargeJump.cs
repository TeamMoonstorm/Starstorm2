using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using Moonstorm.Starstorm2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Skills;

namespace EntityStates.MULE
{
    public class MULEChargeJump : BaseSkillState
    {
        public static float baseChargeDuration = 1.25f;
        private float chargeDuration;
        private float debuffTimer = 0f;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            chargeDuration = baseChargeDuration / attackSpeedStat;
            animator = GetModelAnimator();
            characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 0);
            PlayAnimation();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            if (!isGrounded)
                outer.SetNextStateToMain();

            //this definitely needs a use case ... but what?
            //a super kick downwards that spikes?
            //a slam seems repetitive ...
            //an air-dash ..? maybe repetitive next to spin
            //hmm ...

            debuffTimer += fixedAge;
            if (debuffTimer >= chargeDuration * 0.2f)
            {
                debuffTimer = 0f;

                if (isAuthority)
                    characterBody.AddBuff(SS2Content.Buffs.bdHiddenSlow20);
            }

            characterBody.SetSpreadBloom(Util.Remap(charge, 0f, 1f, 0f, 3f), true);

            StartAimMode();

            if (isAuthority && (charge >= 1f || (!IsKeyDownAuthority() && fixedAge >= 0.01f)))
            {
                Debug.Log("Released at: " + charge);
                MULEJump nextState = new MULEJump();
                nextState.charge = charge;
                outer.SetNextState(nextState);
            }
        }
        
        public void PlayAnimation()
        {
            //PlayCrossfade("Gesture, Override", "ChargeSlam", "Primary.playbackRate", chargeDuration, 0.1f);
        }

        protected float CalcCharge()
        {
            return Mathf.Clamp01(fixedAge / chargeDuration);
        }

        public override void OnExit()
        {
            Debug.Log("Exiting!");
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
