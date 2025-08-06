using SS2;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.Nuke
{
    //Apply surge buff, nothing fancy honestly.
    public class ApplySurge : GenericCharacterMain
    {
        public static float baseDuration;

        private float _duration;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            PlayAnimation("UpperBody, Override", "ApplySurge", "applySurge.playbackRate", _duration);
            characterBody.AddTimedBuff(SS2Content.Buffs.bdNukeSpecial, 10f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge > _duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}