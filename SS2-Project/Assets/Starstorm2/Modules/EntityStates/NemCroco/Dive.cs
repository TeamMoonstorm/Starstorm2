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
        private static float aimVelocity = 2f;
        private static float upwardVelocity = 8f;
        private static float forwardVelocity = 4f;
        private static float minimumY = 0.05f;
        private static float verticalAcceleration = 45f;

        private static float minYVelocityForAnim = 30f;
        private static float maxYVelocityForAnim = -30f;

        private static float verticalSpeedFromGroundedTEMP = 45f;

        private static string leapSoundString = "Play_acrid_shift_jump";
        private static string soundLoopStartEvent = "Play_acrid_shift_fly_loop";
        private static string soundLoopStopEvent = "Stop_acrid_shift_fly_loop";
        private static string collisionString = "DiveCollision";
        private static bool enemyCollisionActive = true;
        private static bool enemyCollisionFliersOnly = true;

        private static float reswimUpwardVelocity = 16f;
        private static float reswimForwardVelocity = 6f;
        public bool enteredFromSwim = false;
        public float swimAge = 0f;
        public int reSwimCount;
       

        private Transform collisionTransform;
        private float previousAirControl;
        private bool detonateNextFrame;
        private Vector3 previousVelocity;
        private bool isGroundedTEMPSHIT; // if the state is started from the ground, we want to instantly start a swim with some bonus speed.
                                         // ideally skilldef just goes straight to swim state instead of this
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        public override void OnEnter()
        {
            base.OnEnter();

            previousAirControl = characterMotor.airControl;
            characterMotor.airControl = airControl;

            isGroundedTEMPSHIT = isGrounded && !enteredFromSwim;
            if (isGroundedTEMPSHIT)
            {
                outer.SetNextState(new Swim { entryVerticalSpeed = verticalSpeedFromGroundedTEMP });
                return;
            }

            Vector3 aimVector = GetAimRay().direction;
            if (isAuthority)
            {
                characterBody.isSprinting = true;
                aimVector.y = Mathf.Max(aimVector.y, minimumY);
                Vector3 aimVelocityVector = aimVector.normalized * aimVelocity * moveSpeedStat;
                Vector3 upwardVelocityVector = Vector3.up * upwardVelocity;
                Vector3 forwardVelocityVector = new Vector3(aimVector.x, 0f, aimVector.z).normalized * forwardVelocity;

                if (enteredFromSwim)
                {
                    aimVelocityVector = Vector3.zero;
                    upwardVelocityVector = Vector3.up * reswimUpwardVelocity;
                    forwardVelocityVector = new Vector3(aimVector.x, 0f, aimVector.z).normalized * reswimForwardVelocity;
                }

                characterMotor.Motor.ForceUnground(0.1f);
                characterMotor.velocity.y = 0f;
                characterMotor.velocity += aimVelocityVector + upwardVelocityVector + forwardVelocityVector;
                // TODO: DOT PRODUCT, APPLY MORE VELOCITY FORWARDS IF WE ARE AIMING DIRECTLY AWAY FROM CURRENT VELOCITY
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

            gameObject.layer = LayerIndex.GetAppropriateFakeLayerForTeam(teamComponent.teamIndex).intVal;
            characterMotor.Motor.RebuildCollidableLayers(); // we only want to collide with flying enemies
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
            if (isAuthority && characterMotor && !isGroundedTEMPSHIT)
            {
                bool hasCollided = CheckCollision();
                characterMotor.moveDirection = inputBank.moveVector;
                characterMotor.velocity += Physics.gravity.normalized * verticalAcceleration * Time.fixedDeltaTime;

                if (fixedAge >= minimumDuration && (hasCollided || isGrounded || detonateNextFrame || (characterMotor.Motor.GroundingStatus.IsStableOnGround && !characterMotor.Motor.LastGroundingStatus.IsStableOnGround)))
                {
                    var swimState = new Swim { entryVerticalSpeed = Mathf.Abs(previousVelocity.y), swimAge = swimAge, reSwimCount = reSwimCount };
                    outer.SetNextState(swimState);
                }

                previousVelocity = characterMotor.velocity;
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
            gameObject.layer = LayerIndex.GetAppropriateLayerForTeam(teamComponent.teamIndex);
            characterMotor.Motor.RebuildCollidableLayers();

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
