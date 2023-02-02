namespace EntityStates.Executioner2
{
    public class AimIonGunOld : BaseSkillState
    {
        public static float baseDuration = 0.1f;

        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / characterBody.attackSpeed;
            PlayAnimation("Gesture, Override", "FireIonGunStart", "Secondary.playbackRate", duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (duration >= fixedAge && isAuthority)
            {
                FireIonGunOld nextState = new FireIonGunOld();
                nextState.activatorSkillSlot = activatorSkillSlot;
                outer.SetNextState(nextState);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }

}