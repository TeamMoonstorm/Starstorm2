using BepInEx;
using EntityStates.DroneTable;
using Moonstorm.Starstorm2;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.DroneTable
{
    public class DestroyLeadin : DroneTableBaseState
    {
        public static float duration;

        public int droneIndex;

        public int itemIndex;

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
            if (NetworkServer.active && fixedAge > duration)
            {
                DestroyAction nextState = new DestroyAction();
                //nextState.droneObject = this.droneObject;
                nextState.itemIndex = this.itemIndex;
                nextState.droneIndex = this.droneIndex;
                //nextState.index = this.index;
                outer.SetNextState(nextState);
            }           
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(droneIndex);
            writer.Write(itemIndex);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            droneIndex = reader.ReadInt32();
            itemIndex = reader.ReadInt32();
        }

    }
}
