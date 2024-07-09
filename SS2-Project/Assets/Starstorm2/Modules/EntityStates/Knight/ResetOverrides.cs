using EntityStates;
using EntityStates.Knight;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Starstorm2.Modules.EntityStates.Knight
{
    public class ResetOverrides : BaseState
    {
        public static SkillDef buffedSpecialRef;

        public static SkillDef buffedUtilityRef;

        public static SkillDef buffedPrimaryRef;

        public override void OnEnter()
        {
            base.OnEnter();

            GenericSkill originalSpecialRef = skillLocator.special;
            originalSpecialRef.UnsetSkillOverride(gameObject, buffedSpecialRef, GenericSkill.SkillOverridePriority.Contextual);

            GenericSkill originalPrimaryRef = skillLocator.primary;
            originalPrimaryRef.UnsetSkillOverride(gameObject, buffedPrimaryRef, GenericSkill.SkillOverridePriority.Contextual);

            GenericSkill originalUtilityRef = skillLocator.utility;
            originalUtilityRef.UnsetSkillOverride(gameObject, buffedUtilityRef, GenericSkill.SkillOverridePriority.Contextual);
        }

        public override void OnExit()
        {
            base.OnExit();
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
