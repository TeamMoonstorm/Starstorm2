using MSU.Config;
using RoR2;
using SS2;
using UnityEngine;

namespace EntityStates.Knight
{
    public abstract class BaseKnightDashMelee : BaseKnightMeleeAttack
    {
        [SerializeField]
        public float minSpeedCoefficient = 2f; // prev: 7, 8, 10f
        [SerializeField]
        public float maxSpeedCoefficient = 7f; //prev: 5
        [SerializeField]
        public float interruptSpeedCoefficient = 0.2f; //prev: 5

        [SerializeField]
        public AnimationCurve dashAnimationCurve;

        [SerializeField]
        public string dodgeSoundString = "";

        [SerializeField]
        public bool forceUnground = true;

        [SerializeField, Tooltip("locks to x/z plane")]
        public bool lockY;

        [SerializeField, Tooltip("False for direction to be based on input")]
        public bool directionAimOverride;
        [SerializeField, Tooltip("Force body to face the dashing direction.")]
        public bool forceFacingDirection;

        public float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        public Vector3 forwardDirection;
        public Vector3 previousPosition;

        public float rollSpeed;
        protected bool interrupted;

        private float sprintSpeedMultiplier;

        public override void OnEnter()
        {
            if (base.isAuthority)
            {
                DisableCharacterMotorCollision();
                CalculateInitialDirection();

                base.OnEnter();
            }
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


        private void CalculateInitialDirection()
        {
            if (inputBank)
            {
                forwardDirection = GetDashDirection();
            }

            sprintSpeedMultiplier = characterBody.isSprinting ? 1 : characterBody.sprintingSpeedMultiplier;

            RecalculateRollSpeed();

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = forwardDirection * rollSpeed;
            }

            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

            if (forceUnground)
            {
                characterMotor.Motor.ForceUnground();
            }
        }

        private Vector3 GetDashDirection()
        {
            Vector3 aimDirection = inputBank.aimDirection;

            if (directionAimOverride || inputBank.moveVector == Vector3.zero)
            {
                if(lockY)
                {
                    aimDirection.y = 0;
                }
                return aimDirection.normalized;
            }

            //gets movement input direction with respect to aim
            aimDirection.y = 0;
            Vector3 rightDirection = -Vector3.Cross(Vector3.up, aimDirection);
            float angle = Vector3.Angle(inputBank.aimDirection, aimDirection);
            if (inputBank.aimDirection.y < 0) angle = -angle;

            Vector3 FinalDirection = Vector3.Normalize(Quaternion.AngleAxis(angle, rightDirection) * inputBank.moveVector);
            
            if (lockY)
            {
                FinalDirection.y = 0;
            }
            return FinalDirection;
        }

        private void RecalculateRollSpeed()
        {
            rollSpeed = moveSpeedStat * sprintSpeedMultiplier * Mathf.Lerp(minSpeedCoefficient, maxSpeedCoefficient, dashAnimationCurve.Evaluate(Mathf.Clamp01(fixedAge / duration))) * (interrupted ? interruptSpeedCoefficient : 1);
        }

        private void MoveKnight()
        {
            RecalculateRollSpeed();

            if (characterDirection && forceFacingDirection) characterDirection.forward = forwardDirection;
            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, stopwatch / duration);

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

        private int _origLayer;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority)
            {
                if (stopwatch >= duration)
                {
                    //base state sets next state
                    return;
                }
                if (!inHitPause)
                {
                    MoveKnight();
                }
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

            if (stopwatch < duration * 0.9f)
            {
                OnInterrupted();
            }

            MoveKnight();
        }

        protected virtual void OnInterrupted()
        {
            interrupted = true;
            PlayCrossfade("FullBody, Override", "BufferEmpty", 0.1f);
        }
    }
}