using System;
using RoR2;
using UnityEngine;

namespace EntityStates.BuriedTreasure
{
    public class Opening : EntityState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            base.PlayAnimation("Body", Opening.OpeningStateHash, Opening.OpeningParamHash, Opening.duration);
            if (base.sfxLocator)
            {
                Util.PlaySound(base.sfxLocator.openSound, base.gameObject);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= Opening.duration)
            {
                this.outer.SetNextState(new EntityStates.Barrel.Opened());
                return;
            }
        }

        private static float duration = 0.43f;

        private static int OpeningStateHash = Animator.StringToHash("Opening");

        private static int OpeningParamHash = Animator.StringToHash("Opening.playbackRate");
    }
}
