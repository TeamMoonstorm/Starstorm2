using EntityStates;


namespace Assets.Starstorm2.Modules.EntityStates.Commando
{
    public class DirtBomb : GenericProjectileBaseState
    {
        public override void PlayAnimation(float duration)
        {
            if (this.GetModelAnimator())
		{
                PlayAnimation("Gesture, Additive", "ThrowGrenade", "FireFMJ.playbackRate", duration * 2f);
                PlayAnimation("Gesture, Override", "ThrowGrenade", "FireFMJ.playbackRate", duration * 2f);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
