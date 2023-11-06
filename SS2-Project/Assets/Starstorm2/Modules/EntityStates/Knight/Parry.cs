using Moonstorm;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.DamageTypes;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    // Parry state that is entered/triggered from the bdKnightParryBuff code.
    public class Parry : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [TokenModifier("SS2_KNIGHT_SHIELD_BASH_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;
        public int swingSide;

        private GenericSkill originalPrimarySkill;
        private GenericSkill originalSecondarySkill;
        private GenericSkill originalUtilitySkill;
        private GenericSkill originalSpecialSkill;

        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);

            animator = GetModelAnimator();
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("Gesture, Override", "Parry", "Secondary.playbackRate", duration * swingTimeCoefficient, 0.15f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnExit()
        {
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