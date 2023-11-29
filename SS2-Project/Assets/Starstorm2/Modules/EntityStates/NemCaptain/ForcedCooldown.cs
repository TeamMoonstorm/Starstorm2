using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemCaptain
{
    public class ForcedCooldown : BaseState
    {
        public static float dur = 0.4f;

        //maybe play an animation here

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= dur)
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
