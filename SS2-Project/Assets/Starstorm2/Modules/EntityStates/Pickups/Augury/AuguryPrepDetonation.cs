using RoR2;
using UnityEngine;

namespace EntityStates.Pickups.Augury
{
    public class AuguryPrepDetonation : AuguryBaseState
    {
        public static float baseDuration = 10;
        public static float minDuration = 3;
        public static string chargeSound = "";

        public Vector3 scale;
        public TeamIndex detonatorTeamIndex;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            var dur = baseDuration / (attachedBody ? attachedBody.attackSpeed : 1f);
            duration = Mathf.Clamp(dur, minDuration, baseDuration * 2);
            Util.PlaySound(chargeSound, gameObject);
            auguryEffect.BeginScale(scale, duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge >= duration)
            {
                var state = new AuguryDetonation();
                state.attackerTeam = detonatorTeamIndex;
                outer.SetNextState(state);
            }
        }
    }
}