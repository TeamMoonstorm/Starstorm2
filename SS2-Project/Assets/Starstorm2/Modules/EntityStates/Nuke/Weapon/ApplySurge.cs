using Moonstorm.Starstorm2;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Nuke
{
    public class ApplySurge : GenericCharacterMain
    {
        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.AddTimedBuff(SS2Content.Buffs.bdNukeSpecial, 10f);
            outer.SetNextStateToMain();
        }
    }
}