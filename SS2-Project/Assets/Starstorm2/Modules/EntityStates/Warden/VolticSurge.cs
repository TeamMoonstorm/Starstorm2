using SS2;

namespace EntityStates.Warden
{
    public class VolticSurge : BaseSkillState
    {
        private int buffDuration = 7;
        public override void OnEnter()
        {
            base.OnEnter();

            if (base.isAuthority)
            {
                characterBody.AddTimedBuff(SS2Content.Buffs.bdWardenSurgeBuff, buffDuration);
            }
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
