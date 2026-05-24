using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Duke
{
    public class DiveRoll : BaseSkillState
    {
        // Roll phase
        private static float rollDuration = 0.6f;
        private static float rollInitialSpeedCoefficient = 6f;
        private static float rollFinalSpeedCoefficient = 1.5f;

        // Slide phase
        private static float maxSlideDuration = 1.2f;
        private static float slideInitialSpeedCoefficient = 4f;
        private static float slideFinalSpeedCoefficient = 1f;

        private static string rollSoundString = "";

        private float rollSpeed;
        private Vector3 forwardDirection;
        private Vector3 previousPosition;
        private Animator animator;

        private bool inSlidePhase;
        private float slideAge;

        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();

            Util.PlaySound(Commando.DodgeState.dodgeSoundString, gameObject);
            if (rollSoundString.Length > 0)
                Util.PlaySound(rollSoundString, gameObject);

            PlayCrossfade("Body", "Utility", "Utility.rate", rollDuration * 1.25f, 0.05f);
            PlayCrossfade("Gesture, Override", "BufferEmpty", 0.05f);
            PlayCrossfade("FullBody, Override", "BufferEmpty", 0.05f);

            if (NetworkServer.active)
                characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, rollDuration);

            if (isAuthority && inputBank && characterDirection)
            {
                forwardDirection = ((inputBank.moveVector == Vector3.zero) ? characterDirection.forward : inputBank.moveVector).normalized;
            }

            inSlidePhase = false;
            slideAge = 0f;
            RecalculateSpeed();

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = forwardDirection * rollSpeed;
                characterMotor.velocity.y = 0f;
            }

            Vector3 velocity = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - velocity;
        }

        private void RecalculateSpeed()
        {
            if (inSlidePhase)
            {
                float t = Mathf.Clamp01(slideAge / maxSlideDuration);
                rollSpeed = moveSpeedStat * Mathf.Lerp(slideInitialSpeedCoefficient, slideFinalSpeedCoefficient, t);
            }
            else
            {
                rollSpeed = moveSpeedStat * Mathf.Lerp(rollInitialSpeedCoefficient, rollFinalSpeedCoefficient, fixedAge / rollDuration);
            }
        }

        private void TransitionToSlide()
        {
            inSlidePhase = true;
            slideAge = 0f;

            PlayCrossfade("Body", "Slide", 0.1f);
            Util.PlaySound("Play_commando_shift", gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RecalculateSpeed();

            characterBody.isSprinting = true;

            if (isAuthority)
            {
                if (inSlidePhase)
                {
                    slideAge += Time.fixedDeltaTime;

                    // Steer during slide
                    if (inputBank && characterDirection)
                    {
                        Vector3 moveInput = inputBank.moveVector;
                        if (moveInput != Vector3.zero)
                        {
                            forwardDirection = Vector3.Lerp(forwardDirection, moveInput.normalized, 3f * Time.fixedDeltaTime).normalized;
                        }
                    }
                }

                Vector3 normalized = (transform.position - previousPosition).normalized;
                if (characterMotor && characterDirection && normalized != Vector3.zero)
                {
                    Vector3 vector = normalized * rollSpeed;
                    float y = vector.y;
                    vector.y = 0f;
                    float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0f);
                    vector = forwardDirection * d;
                    vector.y += Mathf.Max(y, 0f);
                    characterMotor.velocity = vector;
                }

                if (animator)
                {
                    Vector3 rhs = inputBank ? characterDirection.forward : forwardDirection;
                    Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);
                    float num = Vector3.Dot(forwardDirection, rhs);
                    float num2 = Vector3.Dot(forwardDirection, rhs2);
                    animator.SetFloat("forwardSpeed", num);
                    animator.SetFloat("rightSpeed", num2);
                }

                previousPosition = transform.position;

                // Phase transition and exit logic
                if (!inSlidePhase && fixedAge >= rollDuration)
                {
                    if (inputBank.skill3.down)
                    {
                        TransitionToSlide();
                    }
                    else
                    {
                        outer.SetNextStateToMain();
                        return;
                    }
                }

                if (inSlidePhase)
                {
                    if (!inputBank.skill3.down || slideAge >= maxSlideDuration)
                    {
                        outer.SetNextStateToMain();
                        return;
                    }
                }
            }
        }

        public override void OnExit()
        {
            // Reload M2 stocks
            if (skillLocator && skillLocator.secondary)
            {
                skillLocator.secondary.Reset();
                Debug.Log("[Duke] DiveRoll: Restocked secondary (" + skillLocator.secondary.maxStock + " rounds).");
            }

            if (animator)
            {
                animator.SetBool("isRolling", false);
            }

            if (cameraTargetParams)
                cameraTargetParams.fovOverride = -1f;

            base.OnExit();
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
