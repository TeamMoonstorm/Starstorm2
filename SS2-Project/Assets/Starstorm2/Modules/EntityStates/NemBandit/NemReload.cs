using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace EntityStates.NemBandit
{
    public class NemReload : BaseState
    {
        public static float enterSoundPitch;
        public static float exitSoundPitch;

        public static string enterSoundString;
        public static string exitSoundString;

        public static GameObject reloadEffectPrefab;
        public static string reloadEffectMuzzleString;

        public static float baseDuration;
        private bool hasGivenStock;

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
            Util.PlayAttackSpeedSound(enterSoundString, gameObject, enterSoundPitch);
            //effect
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
                outer.SetNextState(new NemReload());
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
