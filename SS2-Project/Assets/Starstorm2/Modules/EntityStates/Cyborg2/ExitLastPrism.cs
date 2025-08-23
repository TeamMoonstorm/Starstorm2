using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2.Components;
using SS2;
using System.Collections.Generic;
namespace EntityStates.Cyborg2
{
    public class ExitLastPrism : BaseSkillState
    {
        public static float baseDuration = 0.33f;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration;// / attackSpeedStat;
            //string layerName = base.isGrounded ? "FullBody, Override" : "Gesture, Override";
            base.PlayAnimation("Gesture, Override", "EndLaser");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
