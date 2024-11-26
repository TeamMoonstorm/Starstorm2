using MSU;
using R2API;
using RoR2;
using UnityEngine;

namespace EntityStates.Knight
{
    class StunSlash : BaseKnightMeleeAttack
    {
        public static float swingTimeCoefficient;
        public static float TokenModifier_dmgCoefficient => new SwingSword().damageCoefficient;

        public static float baseDurationBeforeInterruptable;
        public static float comboFinisherBaseDurationBeforeInterruptable;
        public static float comboFinisherhitPauseDuration;
        public static float comboFinisherDamageCoefficient;

        public new float baseDuration = 1f;
        public new float duration = 1f;

        private string animationStateName = "SwingSword0";


        private void SetupHitbox()
        {
            //animationStateName = "SwingSword3";
            //muzzleString = "SwingLeft";
            //hitboxGroupName = "SpearHitbox";

            //swingEffectPrefab = SS2.Survivors.Knight.KnightSpinEffect;
            //damageCoefficient = 3.5f;
            //hitStopDuration = 0.02f;

            
            //procCoefficient = 0.7f;
            //bonusForce = Vector3.zero;
            //baseDuration = 1f;

            ////0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            ////for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            //attackStartPercentTime = 0.2f;
            //attackEndPercentTime = 0.7f;

            ////this is the point at which the attack can be interrupted by itself, continuing a combo
            //earlyExitPercentTime = 0.8f;

            //swingSoundString = "NemmandoSwing";
            //hitSoundString = "";
            //playbackRateParam = "Primary.Hitbox";
            //hitEffectPrefab = SS2.Survivors.Knight.KnightHitEffect;

            //addModdedDamageType = true;
            //moddedDamageType = SS2.Survivors.Knight.ExtendedStunDamageType;
        }

        public override void OnEnter()
        {
            if (base.isAuthority)
            {
                SetupHitbox();
                base.OnEnter();
            }
        }

        public override void PlayAttackAnimation()
        {
            if (base.isGrounded & !base.GetModelAnimator().GetBool("isMoving"))
            {
                PlayCrossfade("FullBody, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.08f);
            }
            else
            {
                PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.08f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        //handle in parry state
        //public override void OnExit()
        //{
        //    if (base.isAuthority)
        //    {
        //        GenericSkill primarySkill = skillLocator.primary;
        //        GenericSkill utilitySkill = skillLocator.utility;
        //        GenericSkill specialSkill = skillLocator.special;

        //        primarySkill.UnsetSkillOverride(gameObject, SwingSword.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);
        //        utilitySkill.UnsetSkillOverride(gameObject, SpinUtility.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);
        //        specialSkill.UnsetSkillOverride(gameObject, BannerSpecial.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);
        //    }

        //    base.OnExit();
        //}
    }
}