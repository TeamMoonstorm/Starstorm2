﻿using UnityEngine;

namespace EntityStates.NemCaptain.Weapon
{
    public class DroneA : CallDroneBase
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("drone a!");
        }
    }
}
