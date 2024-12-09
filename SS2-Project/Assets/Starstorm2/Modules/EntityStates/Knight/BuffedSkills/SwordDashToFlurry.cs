using MSU.Config;
using RoR2;
using SS2;

namespace EntityStates.Knight
{
    public class SwordDashToFlurry : ShieldPunch
    {
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float flurryTotalDuration = 0.4f;
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float flurrySwipeDamage = 4f;
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float flurrySwipeDuration = 0.22f;

        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        public override void OnExit()
        {
            base.OnExit();

            characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
        }

        public override void PlayAttackAnimation()
        {
            PlayAnimation("FullBody, Override", "FlurryDash");
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
            base.outer.SetNextState(new Flurry { totalDuration = flurryTotalDuration });
        }

        protected override void SetNextState()
        {
            base.outer.SetNextState(new Flurry { totalDuration = flurryTotalDuration });
        }
    }
}