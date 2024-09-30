using UnityEngine;

namespace EntityStates.NemCaptain.Weapon
{
    public class DroneE : CallDroneBase
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("drone e!");
        }
    }
}
