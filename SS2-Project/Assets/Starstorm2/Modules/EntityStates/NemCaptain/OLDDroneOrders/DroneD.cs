﻿using UnityEngine;

namespace EntityStates.NemCaptain.Weapon
{
    public class DroneD : CallDroneBase
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("drone d!");
        }
    }
}
