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
    class Parry : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [TokenModifier("SS2_KNIGHT_SHIELD_BASH_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float TokenModifier_dmgCoefficient => new ShieldPunch().damageCoefficient;
        public int swingSide;

        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            characterBody.AddBuff(SS2Content.Buffs.bdKnightCharged);

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