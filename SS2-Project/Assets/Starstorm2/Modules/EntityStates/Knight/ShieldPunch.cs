using SS2;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using MSU;

namespace EntityStates.Knight
{
    class ShieldPunch : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;
        public int swingSide;

        public override void OnEnter()
        {
            base.OnEnter();

            skillLocator.primary.UnsetSkillOverride(skillLocator.primary, Shield.skillDef, GenericSkill.SkillOverridePriority.Contextual);

            animator = GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("Gesture, Override", "ShieldPunch", "Primary.playbackRate", duration * swingTimeCoefficient, 0.15f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageType.Stun1s;
        }
    }
}