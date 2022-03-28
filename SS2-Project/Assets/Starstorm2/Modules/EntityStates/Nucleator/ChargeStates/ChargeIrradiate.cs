/*using EntityStates;
using RoR2;

namespace EntityStates.Nucleator
{
    class ChargeIrradiate : NucleatorSkillStateBase
    {
        private uint chargePlayID;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "PrimaryCharge", "Primary.playbackRate", 0.8f * maxChargeTime);
            chargePlayID = Util.PlaySound("NucleatorChargePrimary", gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();

            AkSoundEngine.StopPlayingID(chargePlayID);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((fixedAge >= maxChargeTime || !inputBank || !inputBank.skill1.down) && isAuthority)
            {
                FireIrradiate fireIrradiate = new FireIrradiate();
                fireIrradiate.charge = charge;

                outer.SetNextState(fireIrradiate);
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}*/