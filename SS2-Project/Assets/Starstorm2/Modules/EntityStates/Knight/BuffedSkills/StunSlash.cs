using EntityStates;
using EntityStates.Knight;
using MSU;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Starstorm2.Modules.EntityStates.Knight.BuffedSkills
{
    class StunSlash : BasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public static float swingTimeCoefficient = 1.42f;
        [FormatToken("SS2_KNIGHT_PRIMARY_SWORD_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static GameObject beamProjectile;
        public static SkillDef originalSkillRef;
        public static float TokenModifier_dmgCoefficient => new SwingSword().damageCoefficient;
        public int swingSide;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (base.isAuthority)
            {
                EntityStateMachine weaponEsm = EntityStateMachine.FindByCustomName(gameObject, "Weapon");
                if (weaponEsm != null)
                {
                    weaponEsm.SetNextState(new EntityStates.Knight.ResetOverrides());
                }
            } 
        }

        public override void PlayAnimation()
        {
            // TODO: THis skill is only ever entered once so none of this is needed
            string animationStateName = "SwingSword0";

            switch (swingSide)
            {
                case 0:
                    animationStateName = "SwingSword0";
                    swingEffectMuzzleString = "SwingLeft";
                    break;
                case 1:
                    animationStateName = "SwingSword1";
                    swingEffectMuzzleString = "SwingRight";
                    break;
                case 2:
                    animationStateName = "SwingSword2";
                    swingEffectMuzzleString = "SwingLeft";
                    break;
                case 3:
                    animationStateName = "SwingSword3";
                    swingEffectMuzzleString = "SwingCenter";
                    break;
                default:
                    animationStateName = "SwingSword0";
                    swingEffectMuzzleString = "SwingLeft";
                    break;
            }

            PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.05f);
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

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageType.Stun1s;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}