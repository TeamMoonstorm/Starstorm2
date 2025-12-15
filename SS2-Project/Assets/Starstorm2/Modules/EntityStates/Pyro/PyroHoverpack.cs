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

        private PyroController heat;
        private bool setGravity;
        private float originalGravScale;

        public override void OnEnter()
        {
            base.OnEnter();

            heat = GetComponent<PyroController>();
            if (heat == null)
            {
                SS2Log.Error("PyroHoverpack.OnEnter : Body with no PyroController attempted to enter heat-driven state!");
                return;
            }

            characterBody.AddTimedBuff(RoR2.RoR2Content.Buffs.WhipBoost, 0.5f, 1);

            originalGravScale = characterMotor.gravityScale;

            characterBody.isSprinting = true;
            characterBody.bodyFlags |= RoR2.CharacterBody.BodyFlags.SprintAnyDirection;
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
            characterBody.bodyFlags &= ~RoR2.CharacterBody.BodyFlags.SprintAnyDirection;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
