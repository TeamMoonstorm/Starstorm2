using BepInEx;
using EntityStates.DroneTable;
using Moonstorm.Starstorm2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.DroneTable
{
    public class DestroyLeadin : DroneTableBaseState
    {
        public static float duration;

        public GameObject droneObject;

        public float value;

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
            base.OnEnter();
            SS2Log.Info("leadin enter");
        }   

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration)
            {
                DestroyAction nextState = new DestroyAction();
                nextState.droneObject = this.droneObject;
                nextState.value = this.value;
                outer.SetNextState(nextState);
            }
               
        }

        // Update is called once per frame
        public override void OnExit()
        {
            SS2Log.Info("destroy leadin finished");
            base.OnExit();
        }
    }
}
