using RoR2;
using SS2;
using SS2.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Pyro
{
    public class PyroJumppack : BaseState
    {
        private float duration = 0.2f;

        public static float upwardVelocity;
        public static float forwardVelocity;
        public static float minimumY;
        public static float aimVelocity;
        public static float airControl;

        public static float hoverVelocity;
        public static float hoverAcceleration;

        private float previousAirControl;
        private bool endNextFrame = false;

        private float jetpackStopwatch = 0.1f;
        private float jetpackHeat = 0.05f;
        private float jetpackTimer = 0f;

        private PyroController pc;

        private bool hasJumped = false;
        private bool jets = false;

        public override void OnEnter()
        {
            base.OnEnter();
            {
                previousAirControl = characterMotor.airControl;

                pc = characterBody.GetComponent<PyroController>();

                Vector3 direction = GetAimRay().direction;
                if (isAuthority)
                {
                    characterBody.isSprinting = true;
                    float uv = upwardVelocity;
                    float fv = forwardVelocity;
                    if (characterMotor.isGrounded)
                    {
                        //direction.y = 30f;
                        uv *= 0.75f;
                        fv *= 1.25f;
                    }
                    else
                    {
                        //direction.y = 70f;
                        fv *= 0.75f;
                        uv *= 1.25f;
                    }
                    direction.y = Mathf.Max(direction.y, minimumY);

                    Vector3 a = direction.normalized * aimVelocity * moveSpeedStat;
                    Vector3 b = Vector3.up * uv;
                    Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * fv;
                    characterMotor.Motor.ForceUnground();
                    characterMotor.velocity = a + b + b2;
                }
                characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

                if (isAuthority)
                {
                    characterMotor.onMovementHit += OnMovementHit;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Jump();
            Jets();
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            endNextFrame = true;
        }

        public void Jump()
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

        public void Jets()
        {
            if (inputBank.skill3.down)
            {
                if (isAuthority)
                {
                    float vel = characterMotor.velocity.y;
                    vel = Mathf.MoveTowards(vel, hoverVelocity, hoverAcceleration * Time.fixedDeltaTime);
                    characterMotor.velocity = new Vector3(characterMotor.velocity.x, vel, characterMotor.velocity.z);
                    if (jetpackTimer >= jetpackStopwatch)
                    {
                        jetpackTimer -= jetpackStopwatch;
                        if (pc != null)
                            pc.AddHeat(-jetpackHeat);
                    }
                }

                if (!jets)
                {
                    jets = true;
                    characterMotor.airControl = previousAirControl;
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
            characterBody.SetBuffCount(SS2Content.Buffs.bdPyroJet.buffIndex, 0);
            base.OnExit();
        }
    }
}
