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

        public override void OnExit()
        {
            base.OnExit();
            if (isAuthority && wingsStateMachine)
                wingsStateMachine.SetNextState(new Idle());
            base.OnExit();
        }
    }
}
