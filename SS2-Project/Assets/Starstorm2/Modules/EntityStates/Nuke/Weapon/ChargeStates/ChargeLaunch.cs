using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Nuke
{
    /// <summary>
    /// Implementation of nucleator's alt utility, an impulse that while its less strong than <see cref="ChargeImpulse"/>, you can damage enemies while on its fire state
    /// <br></br>
    /// See <see cref="FireFusionLaunch"/>
    /// </summary>
    public class ChargeLaunch : BaseNukeChargeState
    {
        protected override SS2.Survivors.Nuke.IChargedState GetFireState()
        {
            return new FireFusionLaunch();
        }
    }
}