using RoR2;
using RoR2.Skills;


namespace EntityStates.Knight
{
    public class BaseBuffedKnightSkill : BaseState
    {
        public static SkillDef buffedSpecialRef;
        public static SkillDef buffedUtilityRef;
        public static SkillDef buffedPrimaryRef;

        public void ResetSkills()
        {
            GenericSkill originalSpecialRef = skillLocator.special;
            originalSpecialRef.UnsetSkillOverride(gameObject, buffedSpecialRef, GenericSkill.SkillOverridePriority.Contextual);

            GenericSkill originalPrimaryRef = skillLocator.primary;
            originalPrimaryRef.UnsetSkillOverride(gameObject, buffedPrimaryRef, GenericSkill.SkillOverridePriority.Contextual);

            GenericSkill originalUtilityRef = skillLocator.utility;
            originalUtilityRef.UnsetSkillOverride(gameObject, buffedUtilityRef, GenericSkill.SkillOverridePriority.Contextual);
        }
    }
}
