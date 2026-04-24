using MSU;
using MSU.Config;
using RoR2;
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
        private Transform swordPivot;                       //^ew

        // Multihit info
        [SerializeField]
        //public for hot compile
        public float baseFireFrequency = 2f;
        public float fireFrequency;
        public float fireAge;
        public float fireInterval;
        #region test
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testbaseDuration = 0.69f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testminSpeedCoefficient = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testmaxSpeedCoefficient = 4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testinterruptSpeedCoefficient = 0.4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testFireFrequency = 5f;
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testFireFrequencyBoosted = 10f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testDamage = 2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR), Tooltip("overridden by configs")]
        public static float testDamageBoosted = 4f;
        #endregion test

        public override void OnEnter()
        {
            #region test
            baseDuration = testbaseDuration;
            minSpeedCoefficient = testminSpeedCoefficient;
            maxSpeedCoefficient = testmaxSpeedCoefficient;
            interruptSpeedCoefficient = testinterruptSpeedCoefficient;
            baseFireFrequency = testFireFrequency;
            damageCoefficient = testDamage;
            #endregion test

            base.OnEnter();
            this.attack.damageType.damageSource = DamageSource.Utility;
            this.attack.pushAwayForce = 0f;
            this.attack.physForceFlags = PhysForceFlags.doNotExceed | PhysForceFlags.massIsOne;
            fireFrequency = baseFireFrequency * attackSpeedStat;
            fireInterval = 1 / fireFrequency;

            swordPivot = FindModelChild("HitboxAnchor");
            swordPivot.rotation = Util.QuaternionSafeLookRotation(GetAimRay().direction);
        }

        protected override void ModifyMelee()
        {
            swingEffectPrefab = SS2.Survivors.Knight.KnightSpinEffect;
            hitEffectPrefab = SS2.Survivors.Knight.KnightHitEffect;
        }

        protected override void FireAttack()
        {
            if (!inHitPause)
            {
                fireAge += Time.fixedDeltaTime;
            }

            while ((fireAge >= fireInterval))
            {
                fireAge -= fireInterval;
                attack.ResetIgnoredHealthComponents();
                attack.forceVector = forwardDirection * rollSpeed * fireInterval * 6f;
                PlaySwingEffect();

                if (isAuthority && attack.Fire())
                {
                    OnHitEnemyAuthority();
                }
            }
        }

        public override void PlayAttackAnimation()
        {
            PlayCrossfade("FullBody, Override", "Utility", "Utility.playbackRate", duration * swingTimeCoefficient, duration * 0.15f);   
        }

        public override void OnExit()
        {
            base.OnExit();

            swordPivot.transform.localRotation = Quaternion.identity;
        }
    }
}