using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Nuke.Weapon
{
    public class ChargeLaunch : BaseNukeWeaponChargeState
    {
        protected override BaseNukeWeaponFireState GetFireState()
        {
            return new FireLaunch();
        }
    }
}