using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace EntityStates.NemCaptainDrone
{
    public class EntryState : BaseNemCaptainDroneState
    {
        private float duration;
        public static float baseDuration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
