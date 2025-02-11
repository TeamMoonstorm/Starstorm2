using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using SS2.Components;
using UnityEngine.Networking;

namespace EntityStates.NemCaptain.Stress
{
    public class Understressed : BaseState
    {
        protected NemCaptainController nemCaptainController;

        public override void OnEnter()
        {
            base.OnEnter();
            nemCaptainController = GetComponent<NemCaptainController>();
        }

        public override void FixedUpdate()
        {
            if (true)
            {
                //nemCaptainController.stressStateMachine.SetInterruptState(new Overstressed(), InterruptPriority.PrioritySkill);
            }
        }
    }
}

