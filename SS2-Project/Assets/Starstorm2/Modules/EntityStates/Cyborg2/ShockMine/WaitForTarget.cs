using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using Moonstorm.Starstorm2.Components;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace EntityStates.Cyborg2.ShockMine
{
    public class WaitForTarget : BaseShockMineState
    {
		private protected ProjectileSphereTargetFinder targetFinder;

		public override void OnEnter()
		{
			base.OnEnter();
			if (NetworkServer.active)
			{
				this.targetFinder = base.GetComponent<ProjectileSphereTargetFinder>();
				this.targetFinder.enabled = true;
			}
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
				else if (base.projectileTargetComponent.target)
				{
					entityState = new Unburrow();
				}
				if (entityState != null)
				{
					this.outer.SetNextState(entityState);
				}
			}
		}

		public override void OnExit()
		{
			if (this.targetFinder)
			{
				this.targetFinder.enabled = false;
			}
			base.OnExit();
		}

		protected override bool shouldStick => true;
    }
}
