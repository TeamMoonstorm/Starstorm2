using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Nuke.Weapon
{
    public class ChargeFire : BaseNukeChargeState
    {
        protected override SS2.Survivors.Nuke.IChargedState GetFireState()
        {
            return new FireRadonFire();
        }
    }
}