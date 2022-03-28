/*using EntityStates;

namespace EntityStates.Nucleator
{
    class ChargeQuarantine : NucleatorSkillStateBase
    {
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "SecondaryCharge", "Secondary.playbackRate", 0.9f * maxChargeTime);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((fixedAge >= maxChargeTime || !inputBank || !inputBank.skill2.down) && isAuthority)
            {
                FireQuarantine fireQuarantine = new FireQuarantine();
                fireQuarantine.charge = charge;

                outer.SetNextState(fireQuarantine);
                return;
            }
        }



        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}

*/