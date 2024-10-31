using UnityEngine;
using RoR2;

namespace EntityStates.NemBandit
{
    public class NemBanditReload : BaseState
    {
        public static float enterSoundPitch;
        public static float exitSoundPitch;

        public static string enterSoundString;
        public static string exitSoundString;

        public static float baseDuration;
        private bool hasGivenStock;

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
            Util.PlayAttackSpeedSound(enterSoundString, gameObject, enterSoundPitch);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration / 2f)
                GiveStock();
            if (!isAuthority || fixedAge < duration)
                return;
            if (skillLocator.primary.stock < skillLocator.primary.maxStock)
            {
                outer.SetNextState(new NemBanditReload());
                return;
            }
            Util.PlayAttackSpeedSound(exitSoundString, gameObject.gameObject, exitSoundPitch);
            outer.SetNextStateToMain();   
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void GiveStock()
        {
            if (!hasGivenStock)
            {
                skillLocator.primary.AddOneStock();
                hasGivenStock = true;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
