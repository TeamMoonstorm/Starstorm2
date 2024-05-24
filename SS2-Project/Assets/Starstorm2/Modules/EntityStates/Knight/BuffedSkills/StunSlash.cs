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
            Debug.Log("DEBUGGER The stun slash was entered!!");
            base.OnEnter();
            animator = GetModelAnimator();
        }

        public override void OnExit()
        {
            GenericSkill originalPrimarySkill = skillLocator.primary;
            originalPrimarySkill.UnsetSkillOverride(gameObject, SwingSword.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);
            base.OnExit();
        }

        public override void PlayAnimation()
        {
            string animationStateName = (swingSide == 0) ? "SwingSword1" : "SwingSword2";
            PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.05f);
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