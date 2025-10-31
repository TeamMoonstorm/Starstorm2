using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Nuke.Weapon
{
    /// <summary>
    /// Implementation of nucleator's alt secondary, a double flamethrower.
    /// <br></br>
    /// See <see cref="FireRadonFire"/>
    /// </summary>
    public class ChargeFire : BaseNukeChargeState
    {
        protected override SS2.Survivors.Nuke.IChargedState GetFireState()
        {
            return new FireRadonFire();
        }
    }
}