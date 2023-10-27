using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm;
using RoR2;
using UnityEngine.AddressableAssets;
using Moonstorm.Starstorm2;

namespace EntityStates.NemCommando
{
    public class ReloadGun : BaseState
    {
        private bool hasReloaded;
        public static float baseDuration;
        public static string reloadEffectMuzzleString;
        public static GameObject reloadEffectPrefab;

        public static string enterSoundString;
        public static string exitSoundString;

        public static float exitSoundpitch;
        public static float enterSoundPitch;

        private Animator animator;
        private bool hasEjectedMag = false;

        private float duration
        {
            get
            {
                return (baseDuration / attackSpeedStat ) * skillLocator.secondary.cooldownScale;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();

            PlayCrossfade("Gesture, Override, LeftArm", "LowerGun", "FireGun.playbackRate", duration, 0.3f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //SS2Log.Info("Oooooooohhhhh");

            if (animator.GetFloat("ejectMag") >= 0.1 && !hasEjectedMag)
            {
                hasEjectedMag = true;

                FindModelChild("magParticle").GetComponent<ParticleSystem>().Emit(1);
            }

            if (fixedAge >= duration * 0.6 && !hasReloaded)
            {
                Util.PlayAttackSpeedSound(enterSoundString, gameObject, enterSoundPitch);
                skillLocator.secondary.stock = skillLocator.secondary.maxStock;
                hasReloaded = true;
            }
            if (!isAuthority || fixedAge < duration)
            {
                return;
            }
            if (inputBank.skill2.down)
            {
                outer.SetNextState(new ShootGun2());
                skillLocator.secondary.stock -= 1;
            }
            else
                outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
