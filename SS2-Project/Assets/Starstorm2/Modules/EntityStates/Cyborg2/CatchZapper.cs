using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using EntityStates;

namespace EntityStates.Cyborg2
{
    public class CatchZapper : BaseState
    {
        public static float smallHopVelocity = 12f;
        public override void OnEnter()
        {
            base.OnEnter();

            //sound
            //vfx
            //anim
            //some sort of movement that isnt just a smallhop

            GenericSkill skill = base.skillLocator.FindSkill("SecondaryCharged");
            if (skill)
            {
                skill.AddOneStock();
            }
            if(!base.isGrounded)
                base.SmallHop(base.characterMotor, smallHopVelocity);

            this.outer.SetNextStateToMain();
            return;
        }
    }
}
