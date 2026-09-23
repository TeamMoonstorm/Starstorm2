using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using SS2.Components;
using RoR2.Skills;
using R2API;
namespace EntityStates.NemCroco
{
    public class Swim : BaseCharacterMain
    {
        private static float baseDuration = 2f;
        private static float minimumDuration = 0.4f;
        private static float exitBufferDuration = 0.2f;
        private static float maxGroundDistance = 3f;

        private static float verticalSpeedConversion = 1f;
        private static float maxVerticalSpeed = 90f;
        private static float conversionTurnSpeed = 360f;
        private static float decceleration = 50f;
        private static float acceleration = 30f;
        private static float moveSpeedCoefficient = 1.8f;
        private static float characterDirectionCoefficient = 0.5f;

        private static float maxVelocityForAnim = 70f;
        private static float minVelocityForAnim = 0f;

        private static float trailUpdateInterval = 0.2f;
        private static float trailUpdateDistance = 2f;
        private static float trailRadius = 4f;
        private static float trailHeight = 1f;
        private static float trailDamageCoefficientPerSecond = 1f;
        private static float trailProcCoefficientPerSecond = 2f;
        private static float trailDamageInterval = 0.25f;
        private static float trailLifetime = 5f;
        private static float trailLingerTime = 8f;
        public static GameObject damageTrailPrefab;


        private static float speedForMaxBreach = 90f;
        private static float breachMinimumY = 0.05f;

        private static float breachMinAimVelocity = 1f;
        private static float breachMaxAimVelocity = 4f;

        private static float breachMinForwardVelocity = 2.5f;
        private static float breachMaxForwardVelocity = 5f;

        private static float breachMinUpwardVelocity = 4f;
        private static float breachMaxUpwardVelocity = 12f;

        private static float breachDamageCoefficient = 3f;
        private static float breachProcCoefficient = 1f;
        private static float breachRadius = 8f;
        private static float breachPushForce = 6f;
        private static float breachMinVerticalForce = 7f;
        private static float breachMaxVerticalForce = 20f;
        public static GameObject breachEffectPrefab;
        public static GameObject breachImpactEffectPrefab;
        public static SkillDef breachSkillOverride;

        private static string enterSoundString = "Play_acrid_shift_land";
        private static string soundLoopStartEvent = "Play_acrid_shift_fly_loop";
        private static string soundLoopStopEvent = "Stop_acrid_shift_fly_loop";

        public float entryVerticalSpeed;
        private Vector3 convertedVelocity;
        private Vector3 currentVelocity;
        private Vector3 finalVelocity;
        private float duration;
        private Run.FixedTimeStamp exitBufferTime;
        private NemCrocoDamageTrail damageTrail;
        private bool hasBreached;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(entryVerticalSpeed);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            entryVerticalSpeed = reader.ReadSingle();
        }
        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration;

            GetModelTransform().GetComponent<AimAnimator>().enabled = true;
            PlayCrossfade("Gesture, Override", "Swim", 0.1f);
            //PlayCrossfade("Gesture, AdditiveHigh", "Leap", 0.1f); // TODO: Check what this does on acrid

            Util.PlaySound(enterSoundString, gameObject);
            Util.PlaySound(soundLoopStartEvent, gameObject);

            float speedToConvert = Mathf.Min(entryVerticalSpeed, maxVerticalSpeed);
            convertedVelocity = Vector3.down * speedToConvert * verticalSpeedConversion;
            currentVelocity = characterMotor.velocity;
            currentVelocity.y = 0;

            characterBody.fakeActorCounter++;

            if (damageTrailPrefab)
            {
                damageTrail = GameObject.Instantiate(damageTrailPrefab, transform).GetComponent<NemCrocoDamageTrail>();
                damageTrail.transform.position = characterBody.footPosition;
                damageTrail.owner = gameObject;
                damageTrail.damageUpdateInterval = trailDamageInterval;
                damageTrail.damagePerSecond = trailDamageCoefficientPerSecond * damageStat;
                damageTrail.damageType = DamageTypeCombo.GenericUtility;
                damageTrail.damageType.AddModdedDamageType(SS2.Survivors.NemCroco.RadiationOnHit);
                damageTrail.crit = RollCrit();
                damageTrail.procCoefficientPerSecond = trailProcCoefficientPerSecond;
                damageTrail.pointUpdateInterval = trailUpdateInterval;
                damageTrail.pointUpdateDistance = trailUpdateDistance;
                damageTrail.radius = trailRadius;
                damageTrail.height = trailHeight;
                damageTrail.pointLifetime = trailLifetime;
            }

            if (breachSkillOverride)
            {
                if (skillLocator.secondary) skillLocator.secondary.SetSkillOverride(this, breachSkillOverride, GenericSkill.SkillOverridePriority.Contextual);
                if (skillLocator.utility) skillLocator.utility.SetSkillOverride(this, breachSkillOverride, GenericSkill.SkillOverridePriority.Contextual);
                if (skillLocator.special) skillLocator.special.SetSkillOverride(this, breachSkillOverride, GenericSkill.SkillOverridePriority.Contextual);
            }
        }
        public override void UpdateAnimationParameters()
        {
            base.UpdateAnimationParameters();
            Vector3 horizontalVelocity = new Vector3(estimatedVelocity.x, 0f, estimatedVelocity.z);
            float swimCycle = Mathf.Clamp01(Util.Remap(horizontalVelocity.magnitude, minVelocityForAnim, maxVelocityForAnim, 0f, 1f)) * 0.97f;
            modelAnimator.SetFloat("SwimCycle", swimCycle, 0.1f, Time.deltaTime);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
            {
                ApplyMovement();
                GatherInputs();

                bool exitInput = inputBank.skill2.justPressed || inputBank.skill3.justPressed || inputBank.skill4.justPressed || inputBank.jump.justPressed;
                if (exitInput)
                {
                    exitBufferTime = Run.FixedTimeStamp.now;
                }
                exitInput |= exitBufferTime.timeSince <= exitBufferDuration;

                if (fixedAge >= minimumDuration && (exitInput || fixedAge >= baseDuration))
                {
                    Breach();
                    outer.SetNextStateToMain();
                    return;
                }

                // TODO: STICK TO GROUND BETTER, CHECK FOR GROUND BETTER. WALL RUNNING IS BEST BUT ANNOYING
                if (!isGrounded)
                {
                    if (!Physics.Raycast(characterBody.footPosition, Vector3.down, maxGroundDistance, LayerIndex.world.intVal, QueryTriggerInteraction.Ignore))
                    {
                        outer.SetNextStateToMain();
                        return;
                    }
                }
            }
        }
        private void GatherInputs()
        {
            HandleSkill(skillLocator.primary, ref inputBank.skill1);
        }
        private void HandleSkill(GenericSkill skillSlot, ref InputBankTest.ButtonState buttonState)
        {
            if (skillSlot && buttonState.down)
            {
                if (!skillSlot.mustKeyPress || !buttonState.hasPressBeenClaimed)
                {
                    if (skillSlot.ExecuteIfReady())
                    {
                        buttonState.hasPressBeenClaimed = true;
                    }
                }
            }
        }
        private void ApplyMovement()
        {
            Vector3 moveDirection = inputBank.moveVector;
            if (moveDirection == Vector3.zero) // force a bit of forward movement if no move input
            {
                float t = fixedAge / duration;
                float cdSpeed = Mathf.Lerp(characterDirectionCoefficient, 0f, t);
                moveDirection = cdSpeed * characterDirection.forward;
            }
            Vector3 desiredVelocity = moveDirection * moveSpeedStat * moveSpeedCoefficient;
            currentVelocity = Vector3.MoveTowards(currentVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);

            // rotate the velocity we had when diving into the ground, towards the direction we want to move. As if you are turning your body underwater to change your direction
            convertedVelocity = Vector3.RotateTowards(convertedVelocity, moveDirection, conversionTurnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);
            convertedVelocity = Vector3.MoveTowards(convertedVelocity, Vector3.zero, decceleration * Time.fixedDeltaTime);
            Vector3 bonusHorizontalVelocity = new Vector3(convertedVelocity.x, 0f, convertedVelocity.z);

            finalVelocity = currentVelocity + bonusHorizontalVelocity;
            characterMotor.AddDisplacement(finalVelocity * Time.fixedDeltaTime); //////////////////////////////
            characterDirection.moveVector = finalVelocity;
        }

        private void Breach()
        {
            if (hasBreached)
            {
                return;
            }
            hasBreached = true;

            Vector3 aimVector = GetAimRay().direction;
            Vector3 footPosition = characterBody.footPosition;
            EffectManager.SpawnEffect(breachEffectPrefab, new EffectData
            {
                origin = footPosition,
                rotation = Util.QuaternionSafeLookRotation(characterMotor.velocity)
            }, false);

            if (isAuthority)
            {
                float t = Mathf.Clamp01(finalVelocity.magnitude / speedForMaxBreach);
                float aimVelocity = Mathf.Lerp(breachMinAimVelocity, breachMaxAimVelocity, t);
                float upwardVelocity = Mathf.Lerp(breachMinUpwardVelocity, breachMaxUpwardVelocity, t);
                float forwardVelocity = Mathf.Lerp(breachMinForwardVelocity, breachMaxForwardVelocity, t);

                characterBody.isSprinting = true;
                aimVector.y = Mathf.Max(aimVector.y, breachMinimumY);
                Vector3 aimVelocityVector = aimVector.normalized * aimVelocity * moveSpeedStat;
                Vector3 upwardVelocityVector = Vector3.up * upwardVelocity;
                Vector3 forwardVelocityVector = new Vector3(aimVector.x, 0f, aimVector.z).normalized * forwardVelocity;
                characterMotor.Motor.ForceUnground(0.1f);
                characterMotor.velocity = aimVelocityVector + upwardVelocityVector + forwardVelocityVector;
                
                float verticalForce = Mathf.Lerp(breachMinVerticalForce, breachMaxVerticalForce, t);
                var blastAttack = new BlastAttack
                {
                    attacker = gameObject,
                    baseDamage = damageStat * breachDamageCoefficient,
                    baseForce = breachPushForce,
                    bonusForce = verticalForce * Vector3.up,
                    crit = RollCrit(),
                    damageType = DamageTypeCombo.GenericUtility,
                    falloffModel = BlastAttack.FalloffModel.None,
                    procCoefficient = breachProcCoefficient,
                    radius = breachRadius,
                    position = footPosition,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    impactEffect = EffectCatalog.FindEffectIndexFromPrefab(breachImpactEffectPrefab),
                    teamIndex = teamComponent.teamIndex
                }.Fire();
            }
        }

        public override void ModifyNextState(EntityState nextState)
        {
            if (nextState is ChompLeap)
            {
                Breach();
            }
        }

        public override void OnExit()
        {
            characterBody.fakeActorCounter--;

            Util.PlaySound(soundLoopStopEvent, gameObject);

            PlayCrossfade("FullBody, Override", "BufferEmpty", 0.1f);

            if (damageTrail)
            {
                damageTrail.enabled = false;
                damageTrail.gameObject.AddComponent<DestroyOnTimer>().duration = trailLingerTime;
            }

            if (breachSkillOverride)
            {
                if (skillLocator.secondary) skillLocator.secondary.UnsetSkillOverride(this, breachSkillOverride, GenericSkill.SkillOverridePriority.Contextual);
                if (skillLocator.utility) skillLocator.utility.UnsetSkillOverride(this, breachSkillOverride, GenericSkill.SkillOverridePriority.Contextual);
                if (skillLocator.special) skillLocator.special.UnsetSkillOverride(this, breachSkillOverride, GenericSkill.SkillOverridePriority.Contextual);
            }

            base.OnExit();
        }


    }
}
