using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Nuke
{
    /// <summary>
    /// Implementation of nucleator's default utility, an impulse movement that cannot damage enemies
    /// <br></br>
    /// See <see cref="FireFissionImpulse"/>
    /// </summary>
    public class ChargeImpulse : BaseNukeChargeState
    {
        protected override SS2.Survivors.Nuke.IChargedState GetFireState()
        {
            return new FireFissionImpulse();
        }
    }
}