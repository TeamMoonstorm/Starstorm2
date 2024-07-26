using RoR2.Skills;
using System;

namespace EntityStates.NemKnight
{
    public class SpearPrimary : BasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public void SetStep(int i)
        {
            throw new NotImplementedException();
        }

        public float baseDuration = 0.5f;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
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
