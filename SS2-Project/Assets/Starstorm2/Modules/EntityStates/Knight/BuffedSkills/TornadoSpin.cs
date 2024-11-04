using RoR2;

namespace EntityStates.Knight
{
    public class TornadoSpin : SpinUtility
    {

        public override void OnEnter()
        {
            // This is stupid I gotta fix this
            dmgCoeff = 7.0f;
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
            }

            base.OnExit();
        }
    }
}