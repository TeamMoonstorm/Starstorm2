using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Nuke.Weapon
{
    public class ChargeFart : BaseNukeChargeState
    {
        public static float lookThreshold;

        private Ray aimRay;

        protected override SS2.Survivors.Nuke.IChargedState GetFireState()
        {
            aimRay = GetAimRay();
            var normalizedDirection = aimRay.direction.normalized;

            bool cratering = normalizedDirection.y <= lookThreshold && !characterMotor.isGrounded;

            if (cratering)
            {
                return new FireCraterFart();
            }
            return new FireFart();
        }
    }
}