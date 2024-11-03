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
                if (base.jumpInputReceived && base.characterBody)
                {
                    if (currentWeaponState && currentWeaponState.state is Shield && base.characterMotor.jumpCount < (base.characterBody.maxJumpCount + 1))
                    {
                        base.characterMotor.jumpCount++;
                        outer.SetNextState(new EntityStates.Knight.Roll());
                    }
                }
            }

            base.ProcessJump();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}