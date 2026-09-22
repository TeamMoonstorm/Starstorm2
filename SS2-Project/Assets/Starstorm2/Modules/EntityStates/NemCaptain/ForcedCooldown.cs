namespace EntityStates.NemCaptain
{
    public class ForcedCooldown : BaseState
    {
        public static float baseDuration = 1.0f;
        private float duration;

        //maybe play an animation here

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
