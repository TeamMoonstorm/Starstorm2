using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Nuke
{
    public class ChargeImpulse : BaseNukeChargeState
    {
        protected override SS2.Survivors.Nuke.IChargedState GetFireState()
        {
            return new FireFusionImpulse();
        }
    }
}