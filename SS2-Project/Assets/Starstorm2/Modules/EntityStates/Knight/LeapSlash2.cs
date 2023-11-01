using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Knight
{
    public class LeapSlash2 : BasicMeleeAttack
    {
        public static float swingTimeCoefficient;

        public bool swingSide;

        public float totalSwings;
        public float swingCount;

        public override void OnEnter()
        {
            base.OnEnter();
            totalSwings++;
            animator = GetModelAnimator();
        }

        public override void OnExit()
        {
            if (totalSwings < swingCount)
            {
                LeapSlash2 nextState = new LeapSlash2();
                nextState.totalSwings = totalSwings;
                nextState.swingCount = swingCount;
                nextState.swingSide = !swingSide;
                outer.SetNextState(nextState);
            }
            base.OnExit();
        }
        public override void PlayAnimation()
        {
            string animationStateName = swingSide ? "SwingSword1" : "SwingSword2";
            PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.05f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
