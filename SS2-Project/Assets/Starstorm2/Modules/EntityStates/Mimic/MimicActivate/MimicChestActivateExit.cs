using RoR2;
using SS2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace EntityStates.Mimic
{
    public class MimicChestActivateExit : BaseState
    {
        public static float baseDuration;
        private float duration;
       
        public override void OnEnter()
        {
            duration = baseDuration / attackSpeedStat;
            base.OnEnter();

            PlayCrossfade("FullBody, Override", "ActivateExit", "Activate.playbackRate", duration, 0.05f);
            Util.PlaySound("Play_MULT_shift_hit", gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
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
            return InterruptPriority.PrioritySkill;
        }
    }
}
