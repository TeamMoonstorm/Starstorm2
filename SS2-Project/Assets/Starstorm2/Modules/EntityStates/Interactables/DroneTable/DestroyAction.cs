using EntityStates.DroneTable;
using Moonstorm.Starstorm2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EntityStates.DroneTable
{
    public class DestroyAction : DroneTableBaseState
    {

        public static float duration;

        protected override bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        // Start is called before the first frame update
        public override void OnEnter()
        {
            SS2Log.Info("entered destroy action");
            PlayCrossfade("Body", "Action", "Action.playbackRate", duration, 0.05f);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration)
                outer.SetNextStateToMain();
        }

        // Update is called once per frame
        public override void OnExit()
        {
            SS2Log.Info("destroy action finished");
            outer.SetNextStateToMain();
        }
    }
}
