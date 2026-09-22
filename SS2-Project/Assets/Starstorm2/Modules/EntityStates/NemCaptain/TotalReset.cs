using RoR2;
using UnityEngine;
using SS2;

namespace EntityStates.NemCaptain
{
    public class TotalReset : BaseSkillState
    {
        public static float baseDuration = 1f;
        private float duration;

        public static string enterSoundString;
        public static float enterSoundPitch;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            characterBody.SetAimTimer(duration * 2f);
            characterBody.AddTimedBuffAuthority(SS2Content.Buffs.bdTotalReset.buffIndex, 6f);
            Util.PlayAttackSpeedSound(enterSoundString, gameObject, enterSoundPitch);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
