using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm;
using RoR2;
using UnityEngine.AddressableAssets;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.NemCaptain
{
    public class ReloadMachinePistol : BaseState
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

        private NemCaptainController ncc;

        private float duration
        {
            get
            {
                return (baseDuration / attackSpeedStat);
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();
            ncc = characterBody.GetComponent<NemCaptainController>();

            //PlayCrossfade("Gesture, Override, LeftArm", "LowerGun", "FireGun.playbackRate", duration, 0.03f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            /*if (animator.GetFloat("ejectMag") >= 0.1 && !hasEjectedMag)
            {
                hasEjectedMag = true;

                FindModelChild("magParticle").GetComponent<ParticleSystem>().Emit(1);
            }*/

            if (fixedAge >= duration * 0.8f && !hasReloaded)
            {
                Util.PlayAttackSpeedSound(enterSoundString, gameObject, enterSoundPitch);
                skillLocator.primary.stock = skillLocator.primary.maxStock;
                hasReloaded = true;
            }

            if (!isAuthority || fixedAge < duration)
            {
                return;
            }
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
