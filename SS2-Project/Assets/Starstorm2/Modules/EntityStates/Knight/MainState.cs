using RoR2;
using UnityEngine;

namespace EntityStates.Knight
{
    public class MainState : GenericCharacterMain
    {
        EntityStateMachine currentWeaponState;

        public override void OnEnter()
        {
            base.OnEnter();
            currentWeaponState = EntityStateMachine.FindByCustomName(this.characterBody.gameObject, "Weapon");
        }

        public override void ProcessJump()
        {
            if (base.hasCharacterMotor && !healthComponent.isInFrozenState)
            {
                if (base.jumpInputReceived && base.characterBody && base.characterMotor.jumpCount < base.characterBody.maxJumpCount)
                {
                    if (currentWeaponState && currentWeaponState.state is Shield)
                    {
                        outer.SetNextState(new EntityStates.Knight.Roll());
                    } else
                    {
                        base.ProcessJump();
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}