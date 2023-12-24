using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using EntityStates.Chirr.Wings;

namespace EntityStates.Chirr
{
    public class BugCharacterMain : GenericCharacterMain
    {
        private EntityStateMachine wingsStateMachine;

        public override void OnEnter()
        {
            base.OnEnter();
            wingsStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Wings");
        }

        public override void ProcessJump()
        {
            base.ProcessJump();
            if (hasCharacterMotor && hasInputBank && isAuthority)
            {
                bool isDescending = inputBank.jump.down && characterMotor.velocity.y < 0f && !characterMotor.isGrounded;
                bool isHovering = !wingsStateMachine.IsInMainState();
                if (isDescending && !isHovering)
                    wingsStateMachine.SetNextState(new WingsOn());
            }
        }

        public override void HandleMovements()
        {
            base.HandleMovements();

            // copied code from base.handlemovements
            // needed so chirr can aim forward while flying and sprinting
            bool isHovering = !wingsStateMachine.IsInMainState();

           
            if (isHovering)
            {
                Vector3 vector = (this.moveVector == Vector3.zero) ? base.characterDirection.forward : this.moveVector;
                float num = Vector3.Angle(this.aimDirection, vector);
                float num2 = Mathf.Max(this.aimAnimator.pitchRangeMax + this.aimAnimator.pitchGiveupRange, this.aimAnimator.yawRangeMax + this.aimAnimator.yawGiveupRange);
                bool shouldAim = base.characterBody.aimTimer > 0;
                base.characterDirection.moveVector = ((base.characterBody && shouldAim) ? this.aimDirection : vector); //((base.characterBody && shouldAim && num > num2) ? this.aimDirection : vector);
            }
            
        }

        public override void OnExit()
        {
            base.OnExit();
            if (isAuthority && wingsStateMachine)
                wingsStateMachine.SetNextState(new Idle());
            base.OnExit();
        }
    }
}
