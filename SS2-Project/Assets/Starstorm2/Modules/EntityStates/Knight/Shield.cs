using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Skills;

namespace EntityStates.Knight
{
    public class Shield : BaseState
    {
        public static BuffDef shieldBuff;
        public static BuffDef parryBuff;
        public static float parryDur;
        public static float minDur;
        public static SkillDef skillDef;
        private bool hasParried = false;
        private float stopwatch = 0f;

        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();

            PlayCrossfade("Gesture, Override", "RaiseShield", 0.1f);
            animator.SetBool("shieldUp", true);

            characterBody.SetAimTimer(0.5f);

            //characterBody.AddTimedBuff(parryBuff, 0.1f);
            characterBody.AddBuff(shieldBuff);

            skillLocator.primary.SetSkillOverride(skillLocator.primary, skillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= 0.075f && !hasParried)
            {
                hasParried = true;
                characterBody.AddTimedBuff(parryBuff, parryDur);
            }

            stopwatch += fixedAge;
            if (stopwatch >= 0.25f)
            {
                stopwatch = 0f;
                characterBody.SetAimTimer(0.5f);
            }

            if (fixedAge >= minDur && !inputBank.skill2.down)
                outer.SetNextStateToMain(); 
        }

        public override void OnExit()
        {
            base.OnExit();
            animator.SetBool("shieldUp", false);

            characterBody.RemoveBuff(shieldBuff);

            characterBody.SetAimTimer(0.5f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
