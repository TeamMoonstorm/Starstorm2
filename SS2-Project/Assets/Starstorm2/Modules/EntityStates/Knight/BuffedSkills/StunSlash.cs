using EntityStates;
using EntityStates.Knight;
using MSU;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Starstorm2.Modules.EntityStates.Knight.BuffedSkills
{
    class StunSlash : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1.42f;
        [FormatToken("SS2_KNIGHT_PRIMARY_SWORD_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static GameObject beamProjectile;
        public static float TokenModifier_dmgCoefficient => new SwingSword().damageCoefficient;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
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
            
            outer.SetNextStateToMain();
            base.OnExit();
        }

        public override void PlayAnimation()
        {
            string animationStateName = "SwingSword3";
            swingEffectMuzzleString = "SwingCenter";
            PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.05f);
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageType.Stun1s;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}