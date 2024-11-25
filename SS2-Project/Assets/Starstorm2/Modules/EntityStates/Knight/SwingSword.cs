using MSU;
using MSU.Config;
using RoR2;
using RoR2.Skills;
using SS2;
using UnityEngine;

namespace EntityStates.Knight
{
    class SwingSword : BaseKnightMeleeAttack
    {
        public static float swingTimeCoefficient;
        public static float swingTimeCoefficientFinisher;
        public static GameObject beamProjectile;
        public static SkillDef buffedSkillRef;
        [FormatToken("SS2_KNIGHT_PRIMARY_SWORD_DESC",  FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float TokenModifier_dmgCoefficient => new SwingSword().damageCoefficient;

        public static float baseDurationBeforeInterruptable;
        public static float comboFinisherBaseDurationBeforeInterruptable;
        public static float comboFinisherhitPauseDuration;
        public static float comboFinisherDamageCoefficient;

        //public new float baseDuration = 1f;
        //public new float duration = 1f;

        private bool isComboFinisher => swingIndex == 2;
        private string animationStateName = "SwingSword0";
        private Transform swordPivot;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testHitHop = 1;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testDuration = 1;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testSwingTimeCoefficient = 0.8f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testEarlyExit = 0.8f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testDamage = 3f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testHistop = 0.08f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testFinisherDuration = 1;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testFinisherSwingTimeCoefficient = 0.8f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testFinisherEarlyExit = 0.8f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testFinisherDamage = 5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testFinisherHistop = 0.14f;

        private void SetupHitbox()
        {
            damageType = DamageType.Generic;
            procCoefficient = 1f; //what the fuck 0.7
            bonusForce = Vector3.zero;
            hitSoundString = "";
            playbackRateParam = "Primary.playbackRate";
            hitEffectPrefab = SS2.Survivors.Knight.KnightHitEffect;
            hitHopVelocity = testHitHop;
            switch (swingIndex)
            {
                default:
                case 0:
                    animationStateName = "SwingSword0";
                    muzzleString = "SwingRight";
                    hitboxGroupName = "SwordHitbox";
                    break;
                case 1:
                    animationStateName = "SwingSword1";
                    muzzleString = "SwingLeft";
                    hitboxGroupName = "SwordHitbox";
                    break;
                case 2:
                    animationStateName = "SwingSword3";
                    muzzleString = "SwingLeft";
                    hitboxGroupName = "SpearHitbox";
                    break;
            }

            if (!isComboFinisher)
            {
                swingSoundString = "NemmandoSwing";
                swingEffectPrefab = SS2.Survivors.Knight.KnightSpinEffect;
                damageCoefficient = testDamage;
                hitStopDuration = testHistop;

                baseDuration = testDuration;

                swingTimeCoefficient = testSwingTimeCoefficient;

                attackStartPercentTime = 0.215f * swingTimeCoefficient;
                attackEndPercentTime = 0.4f * swingTimeCoefficient;

                earlyExitPercentTime = testEarlyExit * swingTimeCoefficient/*0.8f*/;
            }
            else
            {
                swingSoundString = "NemmandoSwing";
                swingEffectPrefab = SS2.Survivors.Knight.KnightSpinEffect; //comboFinisherSwingEffectPrefab;

                hitStopDuration = testFinisherHistop/*comboFinisherhitPauseDuration*/;
                damageCoefficient = testFinisherDamage/*comboFinisherDamageCoefficient*/;

                baseDuration = testFinisherDuration;

                swingTimeCoefficient = testFinisherSwingTimeCoefficient;

                attackStartPercentTime = 0.252f * swingTimeCoefficient;
                attackEndPercentTime = 0.6f * swingTimeCoefficient;
                earlyExitPercentTime = testFinisherEarlyExit * swingTimeCoefficient/*0.8f*/;
            }

        }

        public override void OnEnter()
        {
            SetupHitbox();
            swordPivot = FindModelChild("HitboxAnchor");
            base.OnEnter();
        }

        public override void PlayAttackAnimation()
        {
            if (base.isGrounded & !base.GetModelAnimator().GetBool("isMoving"))
            {
                PlayCrossfade("FullBody, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient/*swingTimeCoefficient*/, 0.08f);
            } 
            else
            {
                PlayCrossfade("FullBody, Override", "BufferEmpty", 0.1f);
            }
            PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient/*swingTimeCoefficient*/, 0.08f);

        }

        public override void FixedUpdate()
        {
            Vector3 direction = this.GetAimRay().direction;
            direction.y = Mathf.Max(direction.y, direction.y * 0.5f);
            swordPivot.rotation = Util.QuaternionSafeLookRotation(direction);

            base.FixedUpdate();
        }

        public override void OnExit()
        {
            swordPivot.transform.rotation = Quaternion.identity;
            base.OnExit();
        }
    }
}