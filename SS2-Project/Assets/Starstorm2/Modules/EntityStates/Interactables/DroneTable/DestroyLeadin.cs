using BepInEx;
using EntityStates.DroneTable;
using Moonstorm.Starstorm2;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.DroneTable
{
    public class DestroyLeadin : DroneTableBaseState
    {
        public static float duration;

        public GameObject droneObject;

        public PickupIndex index;

        protected override bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }   

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration)
            {
                DestroyAction nextState = new DestroyAction();
                nextState.droneObject = this.droneObject;
                nextState.index = this.index;
                outer.SetNextState(nextState);
            }           
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
