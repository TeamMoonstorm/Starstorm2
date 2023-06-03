using RoR2;
using RoR2.Skills;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.NemHuntress
{
    public class SwapArrowExplosive : BaseState
    {
        public static float baseDuration = 0.2f;
        public static SkillDef skillDef;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            skillLocator.primary.SetSkillOverride(skillLocator.primary, skillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
