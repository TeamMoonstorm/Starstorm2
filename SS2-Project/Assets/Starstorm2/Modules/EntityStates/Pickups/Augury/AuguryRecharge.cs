using UnityEngine;

namespace EntityStates.Pickups.Augury
{
    public class AuguryRecharge : AuguryBaseState
    {
        public static float BaseDuration = 60f;
        public static float MaxDuration = 90f;
        public static float MinDuration = 30f;

        private float rechargeDuration;

        public override void OnEnter()
        {
            base.OnEnter();
            rechargeDuration = Mathf.Clamp(MinDuration, MaxDuration, attachedBody ? BaseDuration / attachedBody.attackSpeed : BaseDuration);
            auguryEffect.RestoreDefaults();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority)
            {
                if (fixedAge > rechargeDuration)
                {
                    outer.SetNextState(new AuguryIdle());
                }
            }
        }
    }
}
