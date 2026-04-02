using EntityStates.Generic;
using UnityEngine;

namespace EntityStates.Knight
{

    public class ShieldPunchWindDown : BaseState
    {
        private static float baseDuration = 0.25f;
        private static InterruptPriority minimumInterruptPriority = InterruptPriority.Skill;
        protected float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
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
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return minimumInterruptPriority;
        }
    }
}