using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemCaptain.Weapon
{
    public class BaseBuffOrder : BaseSkillState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            OnOrderEffect();
            outer.SetState(new ForcedCooldown());
        }

        public virtual void OnOrderEffect() { }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
