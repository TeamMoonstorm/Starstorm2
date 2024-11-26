using SS2;
using SS2.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.DUT
{
    public class SwapChargeType : BaseSkillState
    {
        public static float baseDuration = 0.1f;
        private DUTController controller;

        public override void OnEnter()
        {
            base.OnEnter();
            controller = characterBody.GetComponent<DUTController>();

            if (controller == null)
                SS2Log.Error("Failed to find DU-T controller on body " + characterBody);

            else
            {
                controller.SwapChargeType();
            }

            Debug.Log(controller.currentChargeType);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                if (fixedAge >= baseDuration)
                {
                    outer.SetNextStateToMain();
                    return;
                }
            }
        }
    }
}