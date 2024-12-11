
using SS2.Components;
using RoR2;
using UnityEngine;
using MSU;
using UnityEngine.Networking;
using SS2;
using RoR2.UI;
using System.Collections.Generic;
namespace EntityStates.Events
{
    public class Calm : GenericWeatherState
    {
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            this.stormController.StartLerp(0, 8f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!NetworkServer.active) return;

            if (stormController.stormStartTime.hasPassed)
            {
                this.stormController.OnStormLevelCompleted();
                outer.SetNextState(new Storm { stormLevel = 1, lerpDuration = 15f });
                return;
            }
        }     

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
