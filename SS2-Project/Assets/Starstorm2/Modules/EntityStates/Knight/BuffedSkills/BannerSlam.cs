using EntityStates;
using UnityEngine;
using RoR2;
using RoR2.Skills;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    public class BannerSlam : BannerSpecial
    {
        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            if (base.isAuthority)
            {
                GenericSkill primarySkill = skillLocator.primary;
                GenericSkill utilitySkill = skillLocator.utility;
                GenericSkill specialSkill = skillLocator.special;

                primarySkill.UnsetSkillOverride(gameObject, SwingSword.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);
                utilitySkill.UnsetSkillOverride(gameObject, SpinUtility.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);
                specialSkill.UnsetSkillOverride(gameObject, BannerSpecial.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);

                specialSkill.DeductStock(1);
            }

            outer.SetNextStateToMain();
            base.OnExit();
        }
    }

}