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
    public class MULEChargeSlam : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        public static float baseChargeDuration = 1.25f;
        private float chargeDuration;
        private float debuffTimer = 0f;
        private Animator animator;
        public int swingSide;

        private bool hasAnimated;

        public override void OnEnter()
        {
            base.OnEnter();
            hasAnimated = false;
            chargeDuration = baseChargeDuration / attackSpeedStat;
            animator = GetModelAnimator();
            characterBody.SetBuffCount(SS2Content.Buffs.bdHiddenSlow20.buffIndex, 0);
        }

        void SteppedSkillDef.IStepSetter.SetStep(int i)
        {
            swingSide = i;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)swingSide);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            swingSide = (int)reader.ReadByte();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();

            debuffTimer += fixedAge;
            if (debuffTimer >= chargeDuration * 0.2f)
            {
                debuffTimer = 0f;

                if (isAuthority)
                    characterBody.AddBuff(SS2Content.Buffs.bdHiddenSlow20);
            }

            if (charge >= 0.125f && !hasAnimated)
            {
                hasAnimated = true;
                PlayAnimation();
            }

            characterBody.SetSpreadBloom(Util.Remap(charge, 0f, 1f, 0f, 3f), true);

            StartAimMode();

            if (isAuthority && (charge >= 1f || (!IsKeyDownAuthority() && fixedAge >= 0.01f)))
            {
                if (charge <= 0.125f)
                {
                    //Punch
                    Debug.Log("Punch " + charge);
                    MULEPunch nextState = new MULEPunch();
                    nextState.swingSide = swingSide;
                    outer.SetNextState(nextState);
                    return;
                }
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
                if (!isGrounded && charge >= 0.125f)
                {
                    //Air Slam
                    Debug.Log("Air Slam " + charge);
                    MULEAirSlam nextState = new MULEAirSlam();
                    nextState.charge = charge;
                    outer.SetNextState(nextState);
                    return;
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
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
