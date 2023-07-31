using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemCaptain.Weapon
{
    public class DroneH : CallDroneBase
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("drone h!");
        }
    }
}
