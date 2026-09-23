using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.NemCroco
{
    public class Dive : BaseCharacterMain
    {
        private static float minimumDuration = 0.3f;
        private static float airControl = 0.15f;
        private static float aimVelocity = 1f;
        private static float upwardVelocity = 4f;
        private static float forwardVelocity = 1.5f;
        private static float minimumY = 0.05f;
        private static float verticalAcceleration = -90f;

        private static float minYVelocityForAnim = 30f;
        private static float maxYVelocityForAnim = -30f;

        private static string leapSoundString = "Play_acrid_shift_jump";
        private static string soundLoopStartEvent = "Play_acrid_shift_fly_loop";
        private static string soundLoopStopEvent = "Stop_acrid_shift_fly_loop";
        private static string collisionString = "DiveCollision";
        private static bool enemyCollisionActive = true;
        private static bool enemyCollisionFliersOnly = true;

        private Transform collisionTransform;
        private float previousAirControl;
        private bool detonateNextFrame;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        public override void OnEnter()
        {
            base.OnEnter();

            previousAirControl = characterMotor.airControl;
            characterMotor.airControl = airControl;

            Vector3 aimVector = GetAimRay().direction;
            if (isAuthority)
            {
                characterBody.isSprinting = true;
                aimVector.y = Mathf.Max(aimVector.y, minimumY);
                Vector3 aimVelocityVector = aimVector.normalized * aimVelocity * moveSpeedStat;
                Vector3 upwardVelocityVector = Vector3.up * upwardVelocity;
                Vector3 forwardVelocityVector = new Vector3(aimVector.x, 0f, aimVector.z).normalized * forwardVelocity;
                characterMotor.Motor.ForceUnground(0.1f);
                characterMotor.velocity = aimVelocityVector + upwardVelocityVector + forwardVelocityVector;
            }
            
            GetModelTransform().GetComponent<AimAnimator>().enabled = true;
            PlayCrossfade("FullBody, Override", "Dive", 0.1f);
            //PlayCrossfade("Gesture, AdditiveHigh", "Leap", 0.1f); // TODO: Check what this does on acrid

            Util.PlaySound(leapSoundString, gameObject);
            Util.PlaySound(soundLoopStartEvent, gameObject);

            characterDirection.moveVector = aimVector;

            if (isAuthority)
            {
                characterMotor.onMovementHit += OnMovementHit;
                collisionTransform = FindModelChild(collisionString);
            }
            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(JunkContent.Buffs.IgnoreFallDamage, 0.25f, 1);
            }

            characterBody.fakeActorCounter++; // we only want to collide with flying enemies
        }

        private void OnMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            detonateNextFrame = true;
        }

        public override void UpdateAnimationParameters()
        {
            base.UpdateAnimationParameters();
            float leapCycle = Mathf.Clamp01(Util.Remap(estimatedVelocity.y, minYVelocityForAnim, maxYVelocityForAnim, 0f, 1f)) * 0.97f;
            modelAnimator.SetFloat("LeapCycle", leapCycle, 0.1f, Time.deltaTime);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && characterMotor)
            {
                bool hasCollided = CheckCollision();
                characterMotor.moveDirection = inputBank.moveVector;
                characterMotor.velocity += Physics.gravity * verticalAcceleration * Time.fixedDeltaTime;

                if (fixedAge >= minimumDuration && (hasCollided || isGrounded || detonateNextFrame || (characterMotor.Motor.GroundingStatus.IsStableOnGround && !characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
                {
                    outer.SetNextStateToMain();
                }
            }

            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(JunkContent.Buffs.IgnoreFallDamage, 0.25f, 1);
            }
        }

        private bool CheckCollision()
        {
            if (!enemyCollisionActive)
            {
                return false;
            }
            Collider[] hits = Physics.OverlapBox(collisionTransform.position, collisionTransform.lossyScale * 0.5f, collisionTransform.rotation, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i])
                {
                    HurtBox hurtBox = hits[i].GetComponent<HurtBox>();

                    if (hurtBox && hurtBox.healthComponent.gameObject != gameObject)
                    {
                        if (!enemyCollisionFliersOnly || !hurtBox.healthComponent.body.isFlying)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override void OnExit()
        {
            characterBody.fakeActorCounter--;

            Util.PlaySound(soundLoopStopEvent, gameObject);
            if (isAuthority)
            {
                characterMotor.onMovementHit -= OnMovementHit;
            }
            characterMotor.airControl = previousAirControl;

            PlayCrossfade("FullBody, Override", "BufferEmpty", 0.1f);
            //PlayCrossfade("Gesture, AdditiveHigh", "BufferEmpty", 0.1f); // TODO: Check what this does on acrid

            base.OnExit();
        }

        
    }
}
