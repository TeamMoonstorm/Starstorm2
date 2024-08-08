using MSU;
using RoR2.Skills;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    class SwingSword : BasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public static float swingTimeCoefficient = 1.42f;
        [FormatToken("SS2_KNIGHT_PRIMARY_SWORD_DESC",  FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static GameObject beamProjectile;
        public static SkillDef buffedSkillRef;
        public static float TokenModifier_dmgCoefficient => new SwingSword().damageCoefficient;
        public int swingSide = 0;

        public static float baseDurationBeforeInterruptable;
        public static float comboFinisherBaseDurationBeforeInterruptable;
        private float durationBeforeInterruptable;
        public static float comboFinisherhitPauseDuration;

        public static float comboFinisherDamageCoefficient;

        private bool isComboFinisher => swingSide == 2;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();

            if (isComboFinisher)
            {
                //swingEffectPrefab = comboFinisherSwingEffectPrefab;
                hitPauseDuration = comboFinisherhitPauseDuration;
                damageCoefficient = comboFinisherDamageCoefficient;
            }

            durationBeforeInterruptable = (isComboFinisher ? (comboFinisherBaseDurationBeforeInterruptable / attackSpeedStat) : (baseDurationBeforeInterruptable / attackSpeedStat));
        }

        public override void PlayAnimation()
        {
           string animationStateName = "SwingSword0";

            switch (swingSide)
            {
                case 0:
                    animationStateName = "SwingSword0";
                    swingEffectMuzzleString = "SwingRight";
                    break;
                case 1:
                    animationStateName = "SwingSword1";
                    swingEffectMuzzleString = "SwingLeft";
                    break;
                case 2:
                    animationStateName = "SwingSword3";
                    swingEffectMuzzleString = "SwingCenter";
                    break;
                default:
                    animationStateName = "SwingSword0";
                    swingEffectMuzzleString = "SwingLeft";
                    break;
            }

            if (base.isGrounded & !base.GetModelAnimator().GetBool("isMoving"))
            {
                PlayCrossfade("FullBody, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.08f);
            } else
            {
                PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.08f);
            }         
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
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (!(base.fixedAge < durationBeforeInterruptable))
            {
                return InterruptPriority.Skill;
            }
            return InterruptPriority.PrioritySkill;
        }
    }
}