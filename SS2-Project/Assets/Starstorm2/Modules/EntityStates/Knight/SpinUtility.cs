using MSU;
using MSU.Config;
using RoR2.Skills;
using SS2;
using UnityEngine;

namespace EntityStates.Knight
{
    public class SpinUtility : BaseKnightDashMelee
    {
        public static float swingTimeCoefficient = 1.63f;
        [FormatToken("SS2_KNIGHT_SPECIAL_SPIN_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float TokenModifier_dmgCoefficient => new SpinUtility().damageCoefficient;
        public static SkillDef buffedSkillRef;              //^ew
        
        // Movement variables
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testbaseDuration = 0.69f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testminSpeedCoefficient = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testmaxSpeedCoefficient = 4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testinterruptSpeedCoefficient = 0.4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testearlyexit = 0.1f;

        public override void OnEnter()
        {
            baseDuration = testbaseDuration;
            minSpeedCoefficient = testminSpeedCoefficient;
            maxSpeedCoefficient = testmaxSpeedCoefficient;
            interruptSpeedCoefficient = testinterruptSpeedCoefficient;

            earlyExitPercentTime = testearlyexit;

            base.OnEnter();
        }

        protected override void ModifyMelee()
        {
            swingEffectPrefab = SS2.Survivors.Knight.KnightSpinEffect;
            hitEffectPrefab = SS2.Survivors.Knight.KnightHitEffect;
        }

        // Multihit info
        private float baseFireFrequency = 2f;
        private float fireFrequency;
        private float fireAge;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority)
            {
                if (fixedAge >= duration)
                {
                    outer.SetNextStateToMain();
                    return;
                }
                MultiHitAttack();
            }
        }

        private void MultiHitAttack()
        {
            fireAge += Time.fixedDeltaTime;
            base.characterBody.SetAimTimer(2f);
            attackSpeedStat = base.characterBody.attackSpeed;
            fireFrequency = baseFireFrequency * attackSpeedStat;

            if ((fireAge >= (1f / fireFrequency)) && base.isAuthority)
            {
                fireAge = 0f;
                attack.ResetIgnoredHealthComponents();
                attack.isCrit = base.characterBody.RollCrit();
                attack.Fire();
            }
        }

        public override void PlayAttackAnimation()
        {
            PlayCrossfade("FullBody, Override", "Utility", "Utility.playbackRate", duration * 1.63f, duration * 0.15f);   
        }
    }
}