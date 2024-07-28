using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Pyro
{
    public class PyroJetpack : BaseState
    {
        public static float baseDuration = 0.5f;
        public static float initialSpeedCoefficient = 6.5f;
        public static float finalSpeedCoefficient = 1.5f;

        public static float upwardVelocity;
        public static float forwardVelocity;
        public static float minimumY;
        public static float aimVelocity;
        public static float airControl;

        private float previousAirControl;
        private bool endNextFrame = false;

        private float dashSpeed;
        private float duration = 0.5f;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        private bool isDashing = false;
        private bool hasJumped = false;
        private float stopwatch;

        public override void OnEnter()
        {
            base.OnEnter();
            {
                if (isAuthority && inputBank && characterDirection)
                {
                    forwardDirection = ((inputBank.moveVector == Vector3.zero) ? characterDirection.forward : inputBank.moveVector).normalized;
                }

                RecalculateDashSpeed();

                previousAirControl = characterMotor.airControl;

                Vector3 velocity = characterMotor ? characterMotor.velocity : Vector3.zero;
                previousPosition = transform.position - velocity;

                if (isAuthority)
                {
                    characterMotor.onMovementHit += OnMovementHit;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch <= 0.2f && !inputBank.skill3.down && !isDashing)
                {
                    isDashing = true;
                    if (characterMotor && characterDirection)
                    {
                        characterMotor.velocity.y = 0f;
                        characterMotor.velocity = forwardDirection * dashSpeed;
                    }
                }
                
                if (stopwatch > 0.2f || isDashing)
                {
                    if (isDashing)
                        Dash();
                    else
                        Jump();
                }

            }
        }

        private void RecalculateDashSpeed()
        {
            dashSpeed = Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public void Dash()
        {
            characterBody.isSprinting = true;
            if (isAuthority)
            {
                Vector3 normalized = (transform.position - previousPosition).normalized;
                if (characterMotor && characterDirection && normalized != Vector3.zero)
                {
                    Vector3 vector = normalized * dashSpeed;
                    float y = vector.y;
                    vector.y = 0f;
                    float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                    vector = forwardDirection * d;
                    vector.y += Mathf.Max(y, 0f);
                    characterMotor.velocity = vector;

                    Vector3 rhs = inputBank ? characterDirection.forward : forwardDirection;
                    Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);
                    float num = Vector3.Dot(forwardDirection, rhs);
                    float num2 = Vector3.Dot(forwardDirection, rhs2);
                    //animator.SetFloat("forwardSpeed", num);
                    //animator.SetFloat("rightSpeed", num2);
                }

                previousPosition = transform.position;
                if (fixedAge >= duration && isAuthority)
                {
                    outer.SetNextStateToMain();
                    return;
                }
            }
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            endNextFrame = true;
        }

        public void Jump()
        {
            if (!hasJumped)
            {
                hasJumped = true;

                Vector3 direction = forwardDirection;
                if (isAuthority)
                {
                    characterBody.isSprinting = true;
                    direction.y = Mathf.Max(direction.y, minimumY);
                    Vector3 a = direction.normalized * aimVelocity * moveSpeedStat;
                    Vector3 b = Vector3.up * upwardVelocity;
                    Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                    characterMotor.Motor.ForceUnground();
                    characterMotor.velocity = a + b + b2;
                }
                characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            }
            else
            {
                if (isAuthority && characterMotor)
                {
                    characterMotor.moveDirection = inputBank.moveVector;
                    if (fixedAge >= duration && (endNextFrame || (characterMotor.Motor.GroundingStatus.IsStableOnGround && !characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
                    {
                        characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
                        outer.SetNextStateToMain();
                    }
                }
            }
        }

        public override void OnExit()
        {
            if (isAuthority)
            {
                characterMotor.onMovementHit -= OnMovementHit;
            }
            characterMotor.airControl = previousAirControl;
            characterBody.isSprinting = false;
            base.OnExit();
        }
    }
}
