using SS2;
using UnityEngine.Networking;

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

            if (NetworkServer.active && base.isAuthority)
            {
                characterBody.AddTimedBuff(SS2Content.Buffs.bdNukeSpecial, 11f);
            }
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