using RoR2;

namespace EntityStates.NemBandit
{
    public class NemBanditEnterReload : BaseState
    {
        public static float baseDuration;
        public static string enterSoundString;
        private float duration
        {
            get
            {
                return baseDuration;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(enterSoundString, gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge > duration)
            {
                outer.SetNextState(new NemBanditReload());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
