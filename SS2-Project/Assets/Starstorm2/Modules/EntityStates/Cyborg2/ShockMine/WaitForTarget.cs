using RoR2.Projectile;
using UnityEngine.Networking;
using SS2;

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
				if (base.TryGetComponent(out this.targetFinder))
					this.targetFinder.enabled = true;
				else
					SS2Log.Warning("WaitForTarget missing ProjectileSphereTargetFinder");
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active)
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
