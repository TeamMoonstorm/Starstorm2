using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using RoR2.Projectile;
using Moonstorm.Starstorm2.Components;
namespace EntityStates.Cyborg2.BloonTrap
{
    public class BaseBloonTrapState : BaseState
    {
        private protected ProjectileStickOnImpact projectileStickOnImpact;
        private protected ProjectileTargetComponent projectileTargetComponent;
        private protected Moonstorm.Starstorm2.Components.BloonTrap bloonTrap;
        public override void OnEnter()
        {
            base.OnEnter();

            this.projectileStickOnImpact = base.GetComponent<ProjectileStickOnImpact>();
            this.bloonTrap = base.GetComponent<Moonstorm.Starstorm2.Components.BloonTrap>();

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
