using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;

namespace EntityStates.NemMerc
{
    public class PullKnife : BaseState
    {
        public static float smallHopVelocity = 8f;
        public static float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            if (base.characterMotor)
                base.SmallHop(base.characterMotor, smallHopVelocity);

            this.outer.SetNextStateToMain();

            //anim
            //sound
        }
    }
}
