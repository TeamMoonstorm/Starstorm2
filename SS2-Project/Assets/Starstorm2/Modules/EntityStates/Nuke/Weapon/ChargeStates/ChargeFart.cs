using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Nuke.Weapon
{
    /// <summary>
    /// Default secondary of nucleator, called a "Fart" as a reference to doom eternal... don't ask about it.
    /// <br></br>
    /// See <see cref="FireFart"/> and <see cref="FireCraterFart"/>
    /// </summary>
    public class ChargeFart : BaseNukeChargeState
    {
        [Tooltip("The threshold for wether the attack should go into the cratering version or regular version")]
        public static float lookThreshold;

        private Ray aimRay;

        protected override SS2.Survivors.Nuke.IChargedState GetFireState()
        {
            aimRay = GetAimRay();
            var normalizedDirection = aimRay.direction.normalized;

            //If we're looking down, fire the cratering version, otherwise fire the regular version
            bool cratering = normalizedDirection.y <= lookThreshold && !characterMotor.isGrounded;

            if (cratering)
            {
                return new FireCraterFart();
            }
            return new FireFart();
        }
    }
}