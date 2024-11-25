using MSU;
using MSU.Config;
using RoR2;
using RoR2.Skills;
using SS2;
using UnityEngine;

namespace EntityStates.Knight
{
    public class SpinUtility : BaseKnightMeleeAttack
    {
        public static float swingTimeCoefficient = 1.63f;
        [FormatToken("SS2_KNIGHT_SPECIAL_SPIN_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float TokenModifier_dmgCoefficient => new SpinUtility().damageCoefficient;
        public static SkillDef buffedSkillRef;              //ew
        
        // Movement variables
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static new float duration = 0.7f; // prev: 1f

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float minSpeedCoefficient = 2f; // prev: 7, 8, 10f
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float maxSpeedCoefficient = 7f; //prev: 5
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float interruptSpeedCoefficient = 0.2f; //prev: 5

        public static AnimationCurve dashAnimationCurve;
        public float hopVelocity = 25f; //prev: 25, 30f
        public string dodgeSoundString = "";
        public float dodgeFOV = SS2.Survivors.Knight.dodgeFOV;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float rollSpeed;
        public Vector3 forwardDirection;
        public Vector3 previousPosition;

        // Damage
        public float dmgCoeff = 5.0f;

        // Multihit info
        private float baseFireFrequency = 2f;
        private float fireFrequency;
        private float fireAge;
        private bool interrupted;
        private float sprintSpeedMultiplier;

        private int _origLayer;

        private void CalculateInitialDirection()
        {
            if (inputBank)
            {
                forwardDirection = GetAimRay().direction;
            }

            sprintSpeedMultiplier = characterBody.isSprinting ? 1 : characterBody.sprintingSpeedMultiplier;

            RecalculateRollSpeed();

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = forwardDirection * rollSpeed;
            }

            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

            characterMotor.Motor.ForceUnground();

            if (!isGrounded)
            {
                SmallHop(characterMotor, hopVelocity);
            }
        }

        private void RecalculateRollSpeed()
        {
            rollSpeed = moveSpeedStat * sprintSpeedMultiplier * Mathf.Lerp(minSpeedCoefficient, maxSpeedCoefficient, dashAnimationCurve.Evaluate(fixedAge / duration)) * (interrupted ? interruptSpeedCoefficient : 1);
        }

        private void MoveKnight()
        {
            RecalculateRollSpeed();

            if (characterDirection) characterDirection.forward = forwardDirection;
            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, fixedAge / duration);


            //Vector3 normalized = (transform.position - previousPosition).normalized;
            //if (characterMotor && characterDirection && normalized != Vector3.zero)
            //{
            //    Vector3 vector = normalized * rollSpeed;
            //    float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
            //    vector = forwardDirection * d;

            //    characterMotor.velocity = vector;
            //}
            characterMotor.velocity = forwardDirection * rollSpeed;
            previousPosition = transform.position;
        }

        private void DisableCharacterMotorCollision()
        {
            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            animator = GetModelAnimator();

            if (characterMotor)
            {
                _origLayer = characterMotor.capsuleCollider.gameObject.layer;
                characterMotor.capsuleCollider.gameObject.layer = LayerIndex.fakeActor.intVal;
                characterMotor.Motor.RebuildCollidableLayers();
            }
        }

        private void EnableCharacterMotorCollision()
        {
            if (characterMotor)
            {
                characterMotor.capsuleCollider.gameObject.layer = _origLayer;
                characterMotor.Motor.RebuildCollidableLayers();
            }

            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
        }

        private void SetupHitbox()
        {
            hitboxGroupName = "BigHitbox";

            damageType = DamageType.Generic;
            damageCoefficient = dmgCoeff;
            procCoefficient = 1f;
            pushForce = 400f;
            bonusForce = Vector3.forward;
            baseDuration = 1f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.1f;
            attackEndPercentTime = 0.7f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.5f;

            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            hitHopVelocity = 9f;

            swingSoundString = "NemmandoSwing";
            hitSoundString = "";
            muzzleString = "SwingCenter";
            playbackRateParam = "Util.Hitbox";
            swingEffectPrefab = SS2.Survivors.Knight.KnightSpinEffect;
            hitEffectPrefab = SS2.Survivors.Knight.KnightHitEffect;
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

        public override void OnEnter()
        {
            if (base.isAuthority)
            {
                DisableCharacterMotorCollision();
                CalculateInitialDirection();
                SetupHitbox();

                base.OnEnter();
            }
        }

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
                MoveKnight();
                MultiHitAttack();
            }
        }


        public override void OnExit()
        {
            base.OnExit();

            if (base.isAuthority)
            {
                if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
                EnableCharacterMotorCollision();
            }

            if (fixedAge < duration*0.9f)
            {
                interrupted = true;
                MoveKnight();
                PlayCrossfade("FullBody, Override", "BufferEmpty", 0.1f);
            }
        }

        public override void PlayAttackAnimation()
        {
            PlayCrossfade("FullBody, Override", "Utility", "Utility.playbackRate", duration * /*swingTimeCoefficient*/1.63f, duration * 0.15f);   
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (fixedAge < 0.2f * duration)
            {
                return InterruptPriority.PrioritySkill;
            }

            return InterruptPriority.Any;
        }
    }
}