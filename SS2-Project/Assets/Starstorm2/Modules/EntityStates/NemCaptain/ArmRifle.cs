using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.NemCaptain
{
    public class ArmRifle : BaseSkillState
    {
        public static float baseArmDuration = 1f;
        private float armDuration;

        public static string enterSoundString;
        public static float enterSoundPitch;

        public override void OnEnter()
        {
            base.OnEnter();
            armDuration = baseArmDuration / attackSpeedStat;
            characterBody.SetAimTimer(armDuration * 2f);
            Util.PlayAttackSpeedSound(enterSoundString, gameObject, enterSoundPitch);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= armDuration)
            {
                outer.SetNextState(new FireRifle());
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
