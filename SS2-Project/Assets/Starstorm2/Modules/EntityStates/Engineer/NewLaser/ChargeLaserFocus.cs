using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.EngiTurret.EngiTurretWeapon;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SS2.Survivors;
using R2API;

namespace EntityStates.Engi
{
    public class ChargeLaserFocus : BaseSkillState
    {
        [SerializeField]
        public string animationLayerName;
        [SerializeField]
        public string animationStateName;
        [SerializeField]
        public string animationPlaybackRateParam;
        [SerializeField]
        public float baseDuration = 2f;
        [SerializeField]
        public float baseCrossfadeDuration = 0.1f;
        [SerializeField]
        public string enterSoundString;

        private float duration;
        private float crossfadeDuration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            crossfadeDuration = baseCrossfadeDuration / attackSpeedStat;
            GetAimRay();
            PlayCrossfade(animationLayerName, animationStateName, animationPlaybackRateParam, duration, crossfadeDuration);
            characterBody.SetAimTimer(3f);
            Util.PlaySound(enterSoundString, base.gameObject);

           
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(base.isAuthority && base.fixedAge > duration)
            {
                outer.SetNextState(new FireLaserFocus{
                    activatorSkillSlot = base.activatorSkillSlot
                });
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
