using RoR2.Projectile;
using SS2;
namespace EntityStates.Cyborg2.ShockMine
{
    public class BaseShockMineState : BaseState
    {
        private protected ProjectileStickOnImpact projectileStickOnImpact;
        private protected ProjectileTargetComponent projectileTargetComponent;
        public override void OnEnter()
        {
            base.OnEnter();

            if (!base.TryGetComponent(out this.projectileTargetComponent))
                SS2Log.Warning("BaseShockMineState: missing ProjectileTargetComponent");
            if (!base.TryGetComponent(out this.projectileStickOnImpact))
            {
                SS2Log.Warning("BaseShockMineState: missing ProjectileStickOnImpact");
                return;
            }

            if (this.projectileStickOnImpact.enabled != this.shouldStick)
            {
                this.projectileStickOnImpact.enabled = this.shouldStick;
            }

        }

        protected virtual bool shouldStick
        {
            get => false;
        }
    }
}
