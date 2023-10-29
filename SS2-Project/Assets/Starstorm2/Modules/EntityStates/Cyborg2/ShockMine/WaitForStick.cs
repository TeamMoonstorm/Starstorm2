using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.Cyborg2.ShockMine
{
    public class WaitForStick : BaseShockMineState
    {
        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.projectileStickOnImpact.stuck)
            {
                this.outer.SetNextState(new Burrow());
            }
        }
        protected override bool shouldStick => true;
    }
}
