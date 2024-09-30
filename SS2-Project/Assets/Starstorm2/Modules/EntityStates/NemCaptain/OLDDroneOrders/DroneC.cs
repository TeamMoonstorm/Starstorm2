using UnityEngine;

namespace EntityStates.NemCaptain.Weapon
{
    public class DroneC : CallDroneBase
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("drone c!");
        }
    }
}
