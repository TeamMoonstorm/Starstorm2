using Moonstorm;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.Knight
{
    // Parry state that is entered/triggered from the bdKnightParryBuff code.
    public class Parry : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [TokenModifier("SS2_KNIGHT_SHIELD_BASH_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;

        public static SkillDef buffedPrimarySkill;
        public static SkillDef buffedUtilitySkill;
        public static SkillDef buffedSpecialSkill;

        public int swingSide;

        private GenericSkill originalPrimarySkill;
        private GenericSkill originalUtilitySkill;
        private GenericSkill originalSpecialSkill;


        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            animator = GetModelAnimator();

            // Grab and set the original skills
            originalPrimarySkill = skillLocator.primary;
            Debug.Log("originalPrimarySkill : " + skillLocator.primary.name);
            originalUtilitySkill = skillLocator.utility;
            Debug.Log("originalUtilitySkill : " + skillLocator.utility.name);
            originalSpecialSkill = skillLocator.special;
            Debug.Log("originalSpecialSkill : " + skillLocator.special.name);

            // Assign the buffed skill versions
            originalPrimarySkill.SetSkillOverride(gameObject, buffedPrimarySkill, GenericSkill.SkillOverridePriority.Replacement);
            originalUtilitySkill.SetSkillOverride(gameObject, buffedUtilitySkill, GenericSkill.SkillOverridePriority.Replacement);
            originalSpecialSkill.SetSkillOverride(gameObject, buffedSpecialSkill, GenericSkill.SkillOverridePriority.Replacement);
            Debug.Log("setting buffed skills");
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("Gesture, Override", "Parry", "Secondary.playbackRate", duration * swingTimeCoefficient, 0.15f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnExit()
        {
            originalPrimarySkill.UnsetSkillOverride(gameObject, buffedPrimarySkill, GenericSkill.SkillOverridePriority.Replacement);
            originalUtilitySkill.UnsetSkillOverride(gameObject, buffedUtilitySkill, GenericSkill.SkillOverridePriority.Replacement);
            originalSpecialSkill.UnsetSkillOverride(gameObject, buffedSpecialSkill, GenericSkill.SkillOverridePriority.Replacement);
            Debug.Log("reset skills to original");

            if (inputBank.skill2.down)
            {
                outer.SetNextState(new Shield());
            }

            characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);

            base.OnExit();
        }
        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageType.Stun1s;
        }
    }
}