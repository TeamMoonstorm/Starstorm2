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
            //duration += .02f;
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
            
            SS2Log.Warning("MimicChestActivateExit Exit ");
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
