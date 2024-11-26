using RoR2;
using UnityEditor;
using UnityEngine;

namespace EntityStates.Knight
{
    public class MainState : GenericCharacterMain
    {
        private EntityStateMachine weaponStateMachine;
        private EntityStateMachine rollStateMachine;

        public override void OnEnter()
        {
            base.OnEnter();
            weaponStateMachine = EntityStateMachine.FindByCustomName(this.characterBody.gameObject, "Weapon");
            rollStateMachine = EntityStateMachine.FindByCustomName(this.characterBody.gameObject, "Roll");
        }


        //public override void ProcessJump()
        //{
        //    if (base.hasCharacterMotor && !healthComponent.isInFrozenState)
        //    {
        //        if (/*base.jumpInputReceived*/base.inputBank.jump.down && base.characterBody && currentWeaponStateMachine.CanInterruptState(InterruptPriority.Any))
        //        {
        //            if (base.characterMotor.jumpCount < (base.characterBody.maxJumpCount + 1))
        //            {
        //                if (!isGrounded)
        //                {
        //                    base.characterMotor.jumpCount++;
        //                }

        //                currentWeaponStateMachine.SetNextState(new EntityStates.Knight.Roll());
        //                return;
        //            }
        //        }
        //    }

        //    base.ProcessJump();
        //}

        public override void ProcessJump()
        {
            if (base.hasCharacterMotor && !healthComponent.isInFrozenState)
            {
                if (base.jumpInputReceived &&
                    base.characterBody &&
                    base.characterMotor.jumpCount < (base.characterBody.maxJumpCount + 1) &&
                    weaponStateMachine &&
                    weaponStateMachine.state is Shield &&
                    weaponStateMachine.CanInterruptState(InterruptPriority.Skill))
                {
                    if (!isGrounded)
                    {
                        base.characterMotor.jumpCount++;
                    }

                    weaponStateMachine.SetNextState(new EntityStates.Knight.Roll());
                    return;

                }
            }

            base.ProcessJump();
        }
    }
}