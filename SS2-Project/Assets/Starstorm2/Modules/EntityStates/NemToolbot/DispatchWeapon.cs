using SS2.Components;
using SS2;
using UnityEngine;

namespace EntityStates.NemToolbot
{
    /// <summary>
    /// Primary skill dispatcher for deployed form.
    /// Reads the currently selected weapon from NemToolbotController
    /// and immediately transitions to the appropriate weapon entity state.
    /// </summary>
    public class DispatchWeapon : BaseSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            if (isAuthority)
            {
                if (!gameObject.TryGetComponent(out NemToolbotController controller))
                {
                    SS2Log.Error("NemToolbot DispatchWeapon: Missing NemToolbotController on " + gameObject.name);
                    outer.SetNextStateToMain();
                    return;
                }

                NemToolbotController.WeaponType weapon = controller.currentWeapon;

                if (!controller.HasAmmo(weapon))
                {
                    outer.SetNextStateToMain();
                    return;
                }

                BaseSkillState nextState = weapon switch
                {
                    NemToolbotController.WeaponType.Shotgun => new FireShotgun(),
                    NemToolbotController.WeaponType.RapidLaser => new FireRapidLaser(),
                    NemToolbotController.WeaponType.GrenadeLauncher => new FireGrenadeLauncher(),
                    NemToolbotController.WeaponType.SniperLaser => new FireSniperLaser(),
                    _ => null
                };
                if (nextState == null)
                {
                    SS2Log.Error("NemToolbot DispatchWeapon: Invalid weapon " + weapon);
                    outer.SetNextStateToMain();
                    return;
                }
                nextState.activatorSkillSlot = activatorSkillSlot;
                outer.SetNextState(nextState);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
}
