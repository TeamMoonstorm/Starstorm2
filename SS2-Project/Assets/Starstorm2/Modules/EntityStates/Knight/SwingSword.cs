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
        //[FormatToken("SS2_KNIGHT_PRIMARY_SWORD_DESC",  FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float TokenModifier_dmgCoefficient => new SwingSword().damageCoefficient;

        public static float swingTimeCoefficient;
        public static float finisherSwingTimeCoefficient;

        public static float finisherAttackStart;
        public static float finisherAttackEnd;
        public static float finisherEarlyExit;
        public static float finisherHitPause;
        public static float finisherDamage;
        public static float finisherBaseDuration;

        private string animationStateName = "SwingSword0";
        private Transform swordPivot;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testDuration = 1;
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testDamage = 3f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testFinisherDuration = 1;
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float testFinisherDamage = 5f;

        private void SetupMelee()
        {
            damageCoefficient = testDamage;
            baseDuration = testDuration;
            finisherDamage = testFinisherDamage;
            finisherBaseDuration= testFinisherDuration;
            switch (swingIndex)
            {
                case 0:
                    animationStateName = "SwingSword0";
                    muzzleString = "Swing1Muzzle";
                    break;
                case 1:
                    animationStateName = "SwingSword1";
                    muzzleString = "Swing2Muzzle";
                    break;
                default:
                case 2:
                    animationStateName = "SwingSword3";
                    muzzleString = "SwingStabMuzzle";
                    hitboxGroupName = "SpearHitbox";

                    swingSoundString = "NemmandoSwing";

                    hitStopDuration = finisherHitPause;
                    damageCoefficient = finisherDamage;
                    baseDuration = finisherBaseDuration;

                    swingTimeCoefficient = finisherSwingTimeCoefficient;
                    attackStartTimeFraction = finisherAttackStart;
                    attackEndTimeFraction = finisherAttackEnd;
                    earlyExitTimeFraction = finisherEarlyExit;
                    break;
            }

            attackStartTimeFraction *= swingTimeCoefficient;
            attackEndTimeFraction *= swingTimeCoefficient;
            earlyExitTimeFraction *= swingTimeCoefficient;
            hitEffectPrefab = SS2.Survivors.Knight.KnightHitEffect;
        }

        public override void OnEnter()
        {
            SetupMelee();
            swordPivot = FindModelChild("HitboxAnchor");
            base.OnEnter();
            this.attack.damageType.damageSource = DamageSource.Primary;
        }

        public override void PlayAttackAnimation()
        {
            if (base.isGrounded & !base.GetModelAnimator().GetBool("isMoving"))
            {
                PlayCrossfade("FullBody, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.08f);
            } 
            else
            {
                PlayCrossfade("FullBody, Override", "BufferEmpty", 0.1f);
            }
            PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.08f);

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
            swordPivot.transform.localRotation = Quaternion.identity;
            base.OnExit();
        }
    }
}