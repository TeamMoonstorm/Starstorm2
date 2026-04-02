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

        private static float swingTimeCoefficient = 0.8f;
        private static float finisherSwingTimeCoefficient = 0.64f;

        private static float finisherAttackStart = 0.33f;
        private static float finisherAttackEnd = 0.5f;
        private static float finisherEarlyExit = 1f;
        private static float finisherHitPause = .12f;
        private static float finisherDamage = 4f;
        private static float finisherBaseDuration = 1.5f;

        private string animationStateName = "SwingSword0";
        private Vector3 attackForceVector;
        private Transform swordPivot;

        private static float testDuration = 1.1f;
        private static float testDamage = 2f;

        private static float testFinisherDuration = 1.5f;
        private static float testFinisherDamage = 4f;

        private static float spikeMaxY = -0.8f;
        private static Vector3 swing1ForceVector = new Vector3(-4.5f, 1f, 3f);
        private static Vector3 swing2ForceVector = new Vector3(4.5f, 1f, 3f);
        private static Vector3 swing3ForceVector = new Vector3(0, 6f, 6f);
        private static float spikeForce = -10f;

        private bool isSpike;
        private void SetupMelee()
        {
            damageCoefficient = testDamage;
            baseDuration = testDuration;
            finisherDamage = testFinisherDamage;
            finisherBaseDuration= testFinisherDuration;
            float swingTime = swingTimeCoefficient;
            switch (swingIndex)
            {
                case 0:
                    animationStateName = "SwingSword0";
                    muzzleString = "Swing1Muzzle";
                    attackForceVector = swing1ForceVector;
                    break;
                case 1:
                    animationStateName = "SwingSword1";
                    muzzleString = "Swing2Muzzle";
                    attackForceVector = swing2ForceVector;
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
                    attackForceVector = swing3ForceVector;

                    swingTime = finisherSwingTimeCoefficient;
                    attackStartTimeFraction = finisherAttackStart;
                    attackEndTimeFraction = finisherAttackEnd;
                    earlyExitTimeFraction = finisherEarlyExit;

                    break;
            }
            if (inputBank.aimDirection.y < spikeMaxY)
            {
                isSpike = true;
            }
            attackStartTimeFraction *= swingTime;
            attackEndTimeFraction *= swingTime;
            earlyExitTimeFraction *= swingTime;
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
        protected override void AuthorityModifyOverlapAttack(OverlapAttack attack)
        {
            base.AuthorityModifyOverlapAttack(attack);

            attack.forceVector = Util.QuaternionSafeLookRotation(characterDirection.forward) * attackForceVector;
            if (isSpike)
            {
                attack.forceVector = Vector3.up * spikeForce;
            }
            attack.physForceFlags = PhysForceFlags.massIsOne | PhysForceFlags.resetVelocity;
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