using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS2.Components;
using SS2;

namespace EntityStates.Pyro
{
    public class PyroHoverpack : BaseState
    {
        public static float hoverHeight = 7f;
        public static float raiseAccel = 40f;
        public static float fallAccel = 15f;
        public static float baseRaiseRate = 6f;
        public static float maxRaise = 8f;
        public static float maxFall = 4f;
        public static float snap = 0.15f;
        public static float gravModifier = 0.4f;
        public static float heatPerTick;
        public static float baseDurationBetweenTicks;

        public static float forwardVelocity;
        public static float upwardVelocity;

        private float heatTimer = 0f;

        private PyroController heat;
        private bool setGravity;
        private float originalGravScale;

        private ParticleSystem hoverL;
        private ParticleSystem hoverR;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<PyroController>();
            if (heat == null)
            {
                SS2Log.Error("PyroHoverpack.OnEnter : Body with no PyroController attempted to enter heat-driven state!");
                return;
            }

            ChildLocator cl = GetModelChildLocator();
            if (cl != null)
            {
                cl.FindChild("HoverLParticles").TryGetComponent(out ParticleSystem left);
                {
                    hoverL = left;
                }
                cl.FindChild("HoverRParticles").TryGetComponent(out ParticleSystem right);
                {
                    hoverR = right;
                }
            }

            if (hoverL != null && hoverR != null)
            {
                hoverL.Play();
                hoverR.Play();
            }

            animator = GetModelAnimator();
            if (animator != null)
            {
                animator.SetBool("isHovering", true);
                PlayCrossfade("Body", "HoverIdle", 0.1f);
            }

            originalGravScale = characterMotor.gravityScale;

            characterBody.isSprinting = true;
            characterBody.bodyFlags |= RoR2.CharacterBody.BodyFlags.SprintAnyDirection;

            Vector3 moveVector = inputBank.moveVector;

            Vector3 moveVelocityVector = moveVector.normalized * 2.5f * (characterBody.moveSpeed + ((moveSpeedStat - characterBody.moveSpeed) * 0.5f));
            Vector3 upwardVelocityVector = Vector3.up * upwardVelocity;
            Vector3 forwardVelocityVector = new Vector3(moveVector.x, 0f, moveVector.z).normalized * forwardVelocity;
            characterMotor.velocity = (moveVelocityVector + upwardVelocityVector + forwardVelocityVector);
        }

        public void SetGravityOverride(bool set)
        {
            setGravity = set;

            if (setGravity)
            {
                characterMotor.gravityScale *= gravModifier;
            }
            else
            {
                characterMotor.gravityScale = originalGravScale;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (inputBank.skill3.down == false || heat.heat < 1)
            {
                outer.SetNextStateToMain();
                return;
            }

            heatTimer += GetDeltaTime();
            if (heatTimer >= baseDurationBetweenTicks)
            {
                heatTimer -= heatTimer;
                heat.AddHeat(-heatPerTick);
            }

            bool ground = Physics.Raycast(characterBody.corePosition, Vector3.down, out RaycastHit hit, hoverHeight * 3f, RoR2.LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore);

            float err = 0f;

            if (ground)
            {
                 err = hoverHeight - hit.distance; // get how far we are from intended hover
            }
            else
            {
                err = hoverHeight * -3f; // aaiiieeeee theres no floor
            }

            // if we're pretty close to where we want to be, just don't bother - smooths it out
            if (Mathf.Abs(err) < snap)
            {
                err = 0f;
            }

            if (err < 0)
            {
                // if we're above the height we want, just lower gravity
                if (setGravity == false)
                {
                    SetGravityOverride(true);
                }
            }
            else
            {
                // otherwise set our velocity to raise up to the hover
                if (setGravity == true)
                {
                    SetGravityOverride(false);
                }

                float yVel = Mathf.Clamp(err * baseRaiseRate, -maxFall, maxRaise);
                float currentY = rigidbody.velocity.y;

                float accel = (yVel > currentY) ? raiseAccel : fallAccel;

                float newY = Mathf.MoveTowards(currentY, yVel, accel);

                Vector3 v = characterMotor.velocity;
                v.y = newY;
                characterMotor.velocity = v;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            SetGravityOverride(false);
            characterBody.bodyFlags &= ~RoR2.CharacterBody.BodyFlags.SprintAnyDirection;

            if (animator != null) animator.SetBool("isHovering", false);

            if (hoverL != null && hoverR != null)
            {
                hoverL.Stop();
                hoverR.Stop();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
