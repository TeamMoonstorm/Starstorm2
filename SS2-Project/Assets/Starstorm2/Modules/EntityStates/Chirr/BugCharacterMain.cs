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
            bool isHovering = !wingsStateMachine.IsInMainState();
            // if we are hovering, we want the next "jump" input to end the hover instead of jumping
            bool shouldSkipJump = isHovering && SS2.Survivors.Chirr.toggleHover;
            if (!shouldSkipJump)
                base.ProcessJump();

            if (hasCharacterMotor && hasInputBank && isAuthority)
            {
                bool isDescending = inputBank.jump.down && characterMotor.velocity.y < 0f && !characterMotor.isGrounded;
                
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
                bool shouldAim = base.characterBody.aimTimer > 0;
                base.characterDirection.moveVector = ((base.characterBody && shouldAim) ? this.aimDirection : vector);
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
