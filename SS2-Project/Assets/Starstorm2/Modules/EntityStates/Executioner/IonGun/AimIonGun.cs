namespace EntityStates.Executioner
{
    public class AimIonGun : BaseSkillState
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
                FireIonGun nextState = new FireIonGun();
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