using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS2.Components;
using SS2;
using System;
using UnityEngine.Networking;

namespace EntityStates.Pyro
{
    public class PyroHoverpack : BaseState
    {
        public static float snap = 0.15f;
        public static float heatPerTick;
        public static float baseDurationBetweenTicks;

        private static float hoverHeight = 10f;
        private static float hoverDescendVelocity = -2f; // target y velocity when we are above the hover height
        private static float hoverDescendAcceleration = 40f;

        private static float hoverAscendTargetVelocity = 0f; // target y velocity when we are at the hover height
        private static float hoverAscendMaxVelocity = 12f; // target y velocity when we are below the hover height
        private static float hoverAscendAcceleration = 70f;

        private static float startForwardVelocity = 5.5f;
        private static float endForwardVelocity = 1f;
        private static float forwardAcceleration = 51f;
        private static float forwardVelocityDuration = 1.5f;

        private static float jetBuffDuration = 1f;

        public static float upwardVelocity;

        private float heatTimer = 0f;

        private PyroController heat;

        private ParticleSystem hoverL;
        private ParticleSystem hoverR;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<PyroController>();

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

            characterBody.isSprinting = true;
            characterBody.bodyFlags |= RoR2.CharacterBody.BodyFlags.SprintAnyDirection;

            if (NetworkServer.active)
            {
                characterBody.AddBuff(SS2Content.Buffs.bdPyroJet);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

           

            if (isAuthority)
            {
                heatTimer += GetDeltaTime();
                if (heatTimer >= baseDurationBetweenTicks)
                {
                    heatTimer -= heatTimer;
                    heat.AddHeat(-heatPerTick);
                }
            }

            bool ground = Physics.Raycast(characterBody.footPosition, Vector3.down, out RaycastHit hit, hoverHeight * 3f, RoR2.LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore);

            float error = -1f;
            if (ground)
            {
                 error = hoverHeight - hit.distance; // get how far we are from intended hover
            }

            // if we're pretty close to where we want to be, just don't bother - smooths it out
            if (Mathf.Abs(error) < snap)
            {
                error = 0f;
            }

            if (error < 0)
            {
                // if we are above the target height, descend slowly
                if (characterMotor.velocity.y < 0)
                {
                    characterMotor.velocity.y = Mathf.MoveTowards(characterMotor.velocity.y, hoverDescendVelocity, hoverDescendAcceleration * Time.fixedDeltaTime);
                }
            }
            else
            {
                // if we are below the target height, try to reach zero Y velocity at the target height, by ascending quicker the further off we are.
                // (if we are close to the floor, fly up quickly, while slowing down as we approach hover height)
                float ascendVelocity = Mathf.Lerp(hoverAscendTargetVelocity, hoverAscendMaxVelocity, error / hoverHeight);
                characterMotor.velocity.y = Mathf.MoveTowards(characterMotor.velocity.y, ascendVelocity, hoverAscendAcceleration * Time.fixedDeltaTime);
            }

            // apply forward velocity that decreases over time
            // low acceleration, lots of momentum
            if (fixedAge < forwardVelocityDuration)
            {
                float t = fixedAge / forwardVelocityDuration;
                float targetForwardSpeed = Mathf.Lerp(startForwardVelocity, endForwardVelocity, t) * moveSpeedStat;
                Vector3 targetForwardVelocity = inputBank.moveVector * targetForwardSpeed;
                characterMotor.velocity.x = Mathf.MoveTowards(characterMotor.velocity.x, targetForwardVelocity.x, forwardAcceleration * Time.fixedDeltaTime);
                characterMotor.velocity.z = Mathf.MoveTowards(characterMotor.velocity.z, targetForwardVelocity.z, forwardAcceleration * Time.fixedDeltaTime);
            }

            if (isAuthority)
            {
                if (inputBank.skill3.down == false || heat.heat <= 0f)
                {
                    outer.SetNextStateToMain();
                }
            }

        }

        public override void OnExit()
        {
            base.OnExit();
            characterBody.bodyFlags &= ~RoR2.CharacterBody.BodyFlags.SprintAnyDirection;

            if (animator != null) animator.SetBool("isHovering", false);

            if (hoverL != null && hoverR != null)
            {
                hoverL.Stop();
                hoverR.Stop();
            }

            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(SS2Content.Buffs.bdPyroJet);
                characterBody.AddTimedBuff(SS2Content.Buffs.bdPyroJet, jetBuffDuration);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
