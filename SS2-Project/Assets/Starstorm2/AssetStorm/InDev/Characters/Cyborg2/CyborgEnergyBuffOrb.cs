using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2.Orbs;
using RoR2;

namespace Moonstorm.Starstorm2.Components
{
    public class CyborgEnergyBuffOrb : Orb
    {
        public override void OnArrival()
        {
            base.OnArrival();

            if (!this.target) return;

            SkillLocator skillLocator = this.target.healthComponent.body.skillLocator;

            GenericSkill skill = skillLocator.FindSkill("SecondaryCharged");
            if(skill)
            {
                skill.AddOneStock();
            }
        }
    }
}
