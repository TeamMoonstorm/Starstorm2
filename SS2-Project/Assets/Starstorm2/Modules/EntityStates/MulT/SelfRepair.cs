using SS2.Components;

namespace EntityStates.MulT
{
    public class SelfRepair : BaseSkillState
    {
        public float baseDuration = 3f;
        private float duration = 3f;
        private float healthGainPerRepair = 0.5f;
        private float repairLossPerTick = 10f;

        private SelfRepairController selfRepairController;

        public override void OnEnter()
        {
            base.OnEnter();
            selfRepairController = GetComponent<SelfRepairController>();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration && selfRepairController.repair <= repairLossPerTick)
            {
                outer.SetNextStateToMain();
            }

            if (selfRepairController && base.isAuthority && selfRepairController.repair >= repairLossPerTick)
            {
                this.characterBody.healthComponent.HealFraction(healthGainPerRepair, default);
                selfRepairController.AddRepair(-repairLossPerTick);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
