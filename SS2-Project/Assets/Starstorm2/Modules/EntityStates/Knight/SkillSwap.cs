using RoR2;
using UnityEngine;

namespace EntityStates.Knight
{
    // TODO: Delete this
    public class SkillSwap : BaseState
    {
        private GenericSkill originalPrimarySkill;
        private GenericSkill originalUtilitySkill;
        private GenericSkill originalSpecialSkill;

        public GenericSkill buffedPrimarySkill;
        public GenericSkill buffedUtilitySkill;
        public GenericSkill buffedSpecialSkill;

        public override void OnEnter()
        {
            base.OnEnter();

            // Grab and set the original skills
            originalPrimarySkill = skillLocator.primary;
            Debug.Log("originalPrimarySkill : " + skillLocator.primary.name);
            originalUtilitySkill = skillLocator.utility;
            Debug.Log("originalUtilitySkill : " + skillLocator.utility.name);
            originalSpecialSkill = skillLocator.special;
            Debug.Log("originalSpecialSkill : " + skillLocator.special.name);


            // Assign the buffed skill versions
            skillLocator.primary = buffedPrimarySkill;
            skillLocator.utility = buffedUtilitySkill;
            skillLocator.special = buffedSpecialSkill;
            Debug.Log("setting buffed skills");
        }

        public override void OnExit()
        {
            skillLocator.primary = originalPrimarySkill;
            skillLocator.utility = originalUtilitySkill;
            skillLocator.special = originalSpecialSkill;
            Debug.Log("reset skills to original");
            base.OnExit();
        }
    }
}