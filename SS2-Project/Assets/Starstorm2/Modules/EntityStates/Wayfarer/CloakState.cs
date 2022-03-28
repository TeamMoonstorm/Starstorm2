using RoR2;
using UnityEngine;

namespace EntityStates.Wayfarer
{
    class CloakState : BaseSkillState
    {
        public static float baseDuration = 3.0f;

        private Animator animator;
        private float duration;
        private bool hasCloaked;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;
            PlayCrossfade("FullBody, Override", "Cloak", "Cloak.playbackRate", duration, 0.2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hasCloaked && animator.GetFloat("Cloak.active") > 0.5)
            {
                characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, 10f);
                hasCloaked = true;
            }

            if (fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
