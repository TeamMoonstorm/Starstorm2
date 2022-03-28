using Moonstorm.Starstorm2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Nucleator
{
    class ApplyRadionuclideSurge : BaseSkillState
    {
        private float stateDuration = 6F;
        private float buffDuration = 6f;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active) characterBody.AddTimedBuff(SS2Content.Buffs.BuffNucleatorSpecial, buffDuration);
            animator = GetModelAnimator();

            if (animator) animator.SetLayerWeight(animator.GetLayerIndex("Body, Additive"), 1f);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (animator) animator.SetLayerWeight(animator.GetLayerIndex("Body, Additive"), 0f);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= stateDuration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}