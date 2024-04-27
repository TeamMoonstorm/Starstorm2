
using MSU;
using R2API;
using RoR2;
using RoR2.Skills;
using SS2.Survivors;
using UnityEngine.Networking;

namespace EntityStates.Nemmando
{
    class SwingSword : BasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        //swings for 133% of the attack's duration, used for scaling animation time so he doesn't sheath sword
        public static float swingTimeCoefficient = 1.33f;
        [FormatToken("SS2_NEMMANDO_PRIMARY_BLADE_DESCRIPTION", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float TokenModifier_dmgCoefficient => new SwingSword().damageCoefficient;
        public int swingSide;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
        }

        public override void PlayAnimation()
        {
            string animationStateName = (swingSide == 0) ? "Primary1" : "Primary2";
            PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.1f);
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
            return InterruptPriority.Skill;
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            DamageAPI.AddModdedDamageType(overlapAttack, SS2.Survivors.NemCommando.GougeDamageType);
        }
    }
}