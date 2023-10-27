using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.Cyborg2.ShockMine
{
    public class Burrow : BaseShockMineState
    {
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = Burrow.baseDuration;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.isAuthority)
			{
				EntityState entityState = null;
				if (!base.projectileStickOnImpact.stuck)
				{
					entityState = new WaitForStick();
				}
				else if (this.duration <= base.fixedAge)
				{
					entityState = new WaitForTarget();
				}
				if (entityState != null)
				{
					this.outer.SetNextState(entityState);
				}
			}
		}

		protected override bool shouldStick => true;

		public static float baseDuration = 0.25f;

		private float duration;
	}
}
