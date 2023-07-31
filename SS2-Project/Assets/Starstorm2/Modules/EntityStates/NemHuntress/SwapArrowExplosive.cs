using RoR2;
using RoR2.Skills;
using RoR2.Projectile;
using UnityEngine;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.NemHuntress
{
    public class SwapArrowExplosive : BaseState
    {
        public static float baseDuration = 0.2f;
        public static GameObject arrowPrefab;

        private float duration;

        private NemHuntressController nhc;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;

            nhc = characterBody.GetComponent<NemHuntressController>();
            if (nhc != null)
            {
                nhc.currentArrow = arrowPrefab;
            }

            //skillLocator.primary.SetSkillOverride(skillLocator.primary, skillDef, GenericSkill.SkillOverridePriority.Contextual);
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
