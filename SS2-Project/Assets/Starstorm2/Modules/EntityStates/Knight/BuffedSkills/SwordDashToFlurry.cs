using MSU.Config;
using SS2;

namespace EntityStates.Knight
{
    public class SwordDashToFlurry : ShieldPunch
    {
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testTotalDuration = 1.69f;

        //public override void PlayAttackAnimation()
        //{
        //    PlayAnimation("FullBody, Override", "FlurryDash");
        //}

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
            base.outer.SetNextState(new Flurry { totalDuration = testTotalDuration });
        }

        protected override void SetNextState()
        {
            base.outer.SetNextState(new Flurry { totalDuration = testTotalDuration });
        }
    }
}