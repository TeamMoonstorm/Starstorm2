using RoR2.Projectile;
namespace EntityStates.Cyborg2.ShockMine
{
    public class BaseShockMineState : BaseState
    {
        private protected ProjectileStickOnImpact projectileStickOnImpact;
        private protected ProjectileTargetComponent projectileTargetComponent;
        public override void OnEnter()
        {
            base.OnEnter();

            this.projectileTargetComponent = base.GetComponent<ProjectileTargetComponent>();
            this.projectileStickOnImpact = base.GetComponent<ProjectileStickOnImpact>();

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
