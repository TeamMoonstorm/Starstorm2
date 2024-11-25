using EntityStates;
using MSU.Config;
using RoR2;
using SS2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    public class Roll : BaseSkillState
    {
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float duration = 0.6f; // 0.5 was the real nice sweet spot on distance

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float initialSpeedCoefficient = 5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float finalSpeedCoefficient = 2.5f;


        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float interruptible = 0.2f;

        public static float dodgeFOV = SS2.Survivors.Knight.dodgeFOV;

        private float rollSpeed;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            animator.SetBool("isRolling", true);
            
            Util.PlaySound(Commando.DodgeState.dodgeSoundString, gameObject);

            if (isAuthority && inputBank && characterDirection)
            {
                forwardDirection = (inputBank.moveVector == Vector3.zero ? characterDirection.forward : inputBank.moveVector).normalized;
            }

            RecalculateRollSpeed();

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity.y = 0f;
                characterMotor.velocity = forwardDirection * rollSpeed;
            }

            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

            if (isGrounded)
            {
                PlayCrossfade("FullBody, Override", "Roll", "Utility.rate", duration, 0.05f);
            } 
            else
            {
                PlayCrossfade("FullBody, Override", "AirRoll", "Utility.rate", duration, 0.05f);
            }
        }

        private void RecalculateRollSpeed()
        {
            rollSpeed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((inputBank.skill1.down || inputBank.skill3.down || inputBank.skill4.down) && fixedAge >= interruptible * duration)
            {
                outer.SetNextStateToMain();
                return;
            }

            if (isAuthority && fixedAge >= duration)
            {
                outer.SetNextStateToMain();
                return;
            }

            MoveRoll();
        }

        private void MoveRoll()
        {
            RecalculateRollSpeed();

            //if (characterDirection) characterDirection.forward = forwardDirection;
            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, fixedAge / duration);

            Vector3 normalized = (transform.position - previousPosition).normalized;
            if (characterMotor && characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * rollSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                vector = forwardDirection * d;
                vector.y = 0f;

                characterMotor.velocity = vector;
            }
            previousPosition = transform.position;
        }

        public override void OnExit()
        {
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;
            animator.SetBool("isRolling", false);
            fixedAge = duration;
            MoveRoll();
            base.OnExit();
            characterMotor.disableAirControlUntilCollision = false;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(forwardDirection);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            forwardDirection = reader.ReadVector3();
        }
    }
}