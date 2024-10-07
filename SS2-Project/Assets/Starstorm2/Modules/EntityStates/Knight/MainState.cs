using RoR2;
using UnityEngine;

namespace EntityStates.Knight
{
    public class MainState : GenericCharacterMain
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("DEBUGGER OnEnter triggered....");
        }

        public override void ProcessJump()
        {
            EntityStateMachine currentWeaponState = EntityStateMachine.FindByCustomName(this.characterBody.gameObject, "Weapon");

            if (currentWeaponState.state is Shield)
            {
                Debug.Log("DEBUGGER SHIELD IS HELD");
            }

            base.ProcessJump();
        }
    }
}