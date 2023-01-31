using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;

namespace EntityStates.NemBandit
{
    public class NemEnterReload : BaseState
    {
        public static float baseDuration;
        public static string enterSoundString;
        private float duration
        {
            get
            {
                return baseDuration / attackSpeedStat;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            //play animation
            Util.PlaySound(enterSoundString, gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && fixedAge > duration)
            {
                outer.SetNextState(new NemReload());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
