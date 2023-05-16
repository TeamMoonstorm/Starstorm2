using Moonstorm;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using System;
using UnityEngine;
using UnityEngine.Animations;
using System.Runtime.CompilerServices;
using EntityStates;
using RoR2;

namespace EntityStates.MULE
{
    class MULEStunPunch : BasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        [TokenModifier("SS2_MULE_PRIMARY_PUNCH_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float dmgCoefficient;
        public static float swingTimeCoefficient = 1.33f;
        public int swingSide;

        public float spinCount = 1f;
        public override void OnEnter()
        {
            Debug.Log("Stun Punching");
            damageCoefficient = dmgCoefficient * spinCount;
            base.OnEnter();
            animator = GetModelAnimator();
            hitEffectPrefab = Bison.Headbutt.hitEffectPrefab;
            pushAwayForce *= spinCount;
            //SmallHop(characterMotor, 5f);
            forceForwardVelocity = false;
            //forwardVelocityCurve.keys[1].value *= spinCount;
            //forwardVelocityCurve.keys[2].value *= spinCount;
            //forwardVelocityCurve.keys.SetValue(forwardVelocityCurve.keys[1].value * spinCount, 1);
            //forwardVelocityCurve.keys.SetValue(forwardVelocityCurve.keys[2].value * spinCount, 2);
            characterBody.skillLocator.secondary.DeductStock(1);
            if (spinCount > 1)
            {
                skillLocator.secondary.rechargeStopwatch -= ((spinCount - 1));
            }
        }

        public override void PlayAnimation()
        {
            string animationStateName = (swingSide == 0) ? "Punch1" : "Punch2";
            PlayCrossfade("Gesture, Override", animationStateName, "Secondary.playbackRate", duration, 0.1f);
        }

        void SteppedSkillDef.IStepSetter.SetStep(int i)
        {
            swingSide = i;
            swingEffectMuzzleString = (swingSide == 0) ? "SwingLeft" : "SwingRight";
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
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageType.Stun1s;
        }
    }
}
