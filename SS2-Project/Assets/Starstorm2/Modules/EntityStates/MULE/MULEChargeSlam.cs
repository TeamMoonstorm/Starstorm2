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
    public class MULEChargeSlam : BaseSkillState
    {
        public static float baseChargeDuration = 1.25f;
        private float chargeDuration;
        private float debuffTimer = 0f;
        private Animator animator;
        public int swingSide;

        private bool hasAnimated;
        private bool maxCharge = false;
        public static string maxChargeSound;


        public override void OnEnter()
        {
            base.OnEnter();
            hasAnimated = false;
            chargeDuration = baseChargeDuration / attackSpeedStat;
            animator = GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            if (charge >= 0.01f && !hasAnimated)
            {
                hasAnimated = true;
                PlayAnimation();
            }

            if (charge >= 1f)
            {
                charge = 1f;
                if (!maxCharge)
                {
                    maxCharge = true;
                    Util.PlaySound(maxChargeSound, gameObject);
                }
            }

            characterBody.SetSpreadBloom(Util.Remap(charge, 0f, 1f, 0f, 3f), true);

            StartAimMode();

            if (isAuthority && fixedAge >= 0.01f)
            {
                //Spin
                if (!IsKeyDownAuthority())
                {
                    Debug.Log("Key released");
                    MULESpinFling nextState = new MULESpinFling();
                    nextState.charge = charge;
                    outer.SetNextState(nextState);
                    return;
                }

                //Slam Variants
                if (inputBank.skill1.down)
                {
                    if (charge < 1f && isGrounded)
                    {
                        //Normal Slam
                        Debug.Log("Normal Slam " + charge);
                        MULESlam nextState = new MULESlam();
                        nextState.charge = charge;
                        outer.SetNextState(nextState);
                        return;
                    }

                    if (charge >= 1f && isGrounded)
                    {
                        //Mega Slam
                        Debug.Log("Max Slam " + charge);
                        MULESuperSlam nextState = new MULESuperSlam();
                        nextState.charge = charge;
                        outer.SetNextState(nextState);
                        return;
                    }
                    if (!isGrounded)
                    {
                        //Air Slam
                        Debug.Log("Air Slam " + charge);
                        MULEAirSlam nextState = new MULEAirSlam();
                        nextState.charge = charge;
                        outer.SetNextState(nextState);
                        return;
                    }
                }

                //Jump
                if (inputBank.jump.down && isGrounded)
                {
                    Debug.Log("Released at: " + charge);
                    MULEJump nextState = new MULEJump();
                    nextState.charge = charge;
                    outer.SetNextState(nextState);
                }

                Debug.Log("Released at: " + charge);
            }
        }
        
        public void PlayAnimation()
        {
            PlayCrossfade("Gesture, Override", "ChargeSlam", "Primary.playbackRate", chargeDuration, 0.1f);
        }

        protected float CalcCharge()
        {
            return Mathf.Clamp01(fixedAge / chargeDuration);
        }

        public override void OnExit()
        {
            Debug.Log("Exiting!");
            activatorSkillSlot.DeductStock(1);
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
