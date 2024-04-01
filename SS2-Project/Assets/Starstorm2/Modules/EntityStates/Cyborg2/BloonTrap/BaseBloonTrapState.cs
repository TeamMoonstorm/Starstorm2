using RoR2.Projectile;
namespace EntityStates.Cyborg2.BloonTrap
{
    public class BaseBloonTrapState : BaseState
    {
        private protected ProjectileStickOnImpact projectileStickOnImpact;
        private protected ProjectileTargetComponent projectileTargetComponent;
        private protected SS2.Components.BloonTrap bloonTrap;
        public override void OnEnter()
        {
            base.OnEnter();

            this.projectileStickOnImpact = base.GetComponent<ProjectileStickOnImpact>();
            this.bloonTrap = base.GetComponent<SS2.Components.BloonTrap>();

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
