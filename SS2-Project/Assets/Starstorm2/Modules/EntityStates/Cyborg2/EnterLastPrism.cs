using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using SS2.Components;
using SS2;
using System.Collections.Generic;
namespace EntityStates.Cyborg2
{
    public class EnterLastPrism : BaseSkillState
    {
        public static float exitHopVelocity = 12f;
        public static float baseDuration = 1f;
        private float duration;
        private bool canceled = true;
        private bool startGrounded;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / attackSpeedStat;
            string layerName = base.isGrounded ? "FullBody, Override" : "Gesture, Override";
            base.PlayAnimation(layerName, "FireLaser", "Primary.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                canceled = false;
                this.outer.SetNextState(new LastPrism());
                return;
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if(canceled)
            {
                base.PlayAnimation("FullBody, Override", "BufferEmpty");
                base.PlayAnimation("Gesture, Override", "BufferEmpty");
            }

        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
