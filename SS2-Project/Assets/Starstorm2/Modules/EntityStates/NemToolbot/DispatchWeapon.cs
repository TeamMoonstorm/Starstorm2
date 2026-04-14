using SS2.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemToolbot
{
    /// <summary>
    /// Primary skill dispatcher for deployed form.
    /// Reads the currently selected weapon from NemToolbotController
    /// and immediately transitions to the appropriate weapon entity state.
    /// </summary>
    public class DispatchWeapon : BaseState
    {
        private NemToolbotController controller;
        private NemToolbotController.WeaponType cachedWeapon = NemToolbotController.WeaponType.RapidLaser;

        public override void OnEnter()
        {
            base.OnEnter();

            if (isAuthority)
            {
                if (!gameObject.TryGetComponent(out controller))
                {
                    Debug.LogError("NemToolbot DispatchWeapon: Failed to get NemToolbotController on " + gameObject.name);
                    outer.SetNextStateToMain();
                    return;
                }

                cachedWeapon = controller.currentWeapon;
                EntityState nextState = cachedWeapon switch
                {
                    NemToolbotController.WeaponType.Shotgun => new FireShotgun(),
                    NemToolbotController.WeaponType.RapidLaser => new FireRapidLaser(),
                    NemToolbotController.WeaponType.GrenadeLauncher => new FireGrenadeLauncher(),
                    NemToolbotController.WeaponType.SniperLaser => new FireSniperLaser(),
                    _ => new FireRapidLaser()
                };
                outer.SetNextState(nextState);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)cachedWeapon);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            reader.ReadByte();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
}
