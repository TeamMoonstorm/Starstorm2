using UnityEngine.Networking;

namespace EntityStates.Cyborg2.ShockMine
{
    public class WaitForStick : BaseShockMineState
    {
        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && base.projectileStickOnImpact.stuck)
            {
                this.outer.SetNextState(new Burrow());
            }
        }
        protected override bool shouldStick => true;
    }
}
