using MSU.Config;
using RoR2;
using SS2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    public class SwordDashToFlurry : BaseSkillState
    {
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float flurryTotalDuration = 0.4f;
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float flurrySwipeDamage = 4f;
        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR)]
        public static float flurrySwipeDuration = 0.22f;

        private static float baseDuration = 0.2f;
        
        private static float minSpeedCoefficient = 0f;
        private static float maxSpeedCoefficient = 10f;
        private static float interruptSpeedCoefficient = 0.2f;
        private static string collisionTransformString = "ShieldBashHitbox";

        public static AnimationCurve dashAnimationCurve;

        private static string enterSoundString = Commando.DodgeState.dodgeSoundString;

        private static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        public Vector3 forwardDirection;
        public Vector3 previousPosition;

        public float rollSpeed;
        protected bool interrupted;
        private float stopwatch;
        private float duration;

        private float sprintSpeedMultiplier;
        private Transform collisionTransform;

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration; // / attackSpeedStat;

            if (isAuthority)
            {
                collisionTransform = FindModelChild(collisionTransformString);
                CalculateInitialDirection();
            }

            if (NetworkServer.active)
            {
                characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            PlayAnimation("FullBody, Override", "FlurryDash");
            Util.PlaySound(enterSoundString, gameObject);
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

            characterMotor.Motor.ForceUnground();
            
        }

        private Vector3 GetDashDirection()
        {
            return inputBank.aimDirection;
        }

        private void RecalculateRollSpeed()
        {
            rollSpeed = moveSpeedStat * sprintSpeedMultiplier * Mathf.Lerp(minSpeedCoefficient, maxSpeedCoefficient, dashAnimationCurve.Evaluate(Mathf.Clamp01(fixedAge / duration))) * (interrupted ? interruptSpeedCoefficient : 1);
        }

        public virtual void MoveKnight()
        {
            RecalculateRollSpeed();

            if (characterDirection) characterDirection.forward = forwardDirection;
            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(dodgeFOV, 60f, stopwatch / duration);

            characterMotor.velocity = forwardDirection * rollSpeed;
            previousPosition = transform.position;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.isAuthority)
            {
                bool shouldExit = fixedAge > duration || CheckCollisions();
                
                if (shouldExit)
                {
                    outer.SetNextState(new Flurry { totalDuration = flurryTotalDuration });
                    return;
                }
            }
        }

        private bool CheckCollisions()
        {
            if (collisionTransform)
            {
                // fuck nonalloc shit makes no sense
                Collider[] hits = Physics.OverlapBox(collisionTransform.position, collisionTransform.lossyScale * 0.5f, collisionTransform.rotation, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i])
                    {
                        HurtBox hurtBox = hits[i].GetComponent<HurtBox>();

                        if (hurtBox)
                        {
                            if (hurtBox.healthComponent.gameObject != gameObject && FriendlyFireManager.ShouldDirectHitProceed(hurtBox.healthComponent, teamComponent.teamIndex))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public override void OnExit()
        {
            base.OnExit();

            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
            if (cameraTargetParams) cameraTargetParams.fovOverride = -1f;

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
