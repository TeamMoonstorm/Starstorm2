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
        private static float baseDuration = 1.5f;
        private static float minimumDuration = 0.4f;
        private static float exitBufferDuration = 0.2f;
        private static float maxGroundDistance = 12f;

        private static float verticalSpeedConversion = 1f;
        private static float maxVerticalSpeed = 90f;
        private static float conversionTurnSpeed = 360f;
        private static float decceleration = 70f;
        private static float acceleration = 45f;
        private static float moveSpeedCoefficient = 2.5f;
        private static float characterDirectionCoefficient = 0.5f;
        private static float gravityCoefficient = 1.6f;

        private static float maxVelocityForAnim = 70f;
        private static float minVelocityForAnim = 0f;

        private static float trailUpdateInterval = 11f;
        private static float trailUpdateDistance = 4f;
        private static float trailRadius = 4f;
        private static float trailHeight = 1f;
        private static float trailDamageCoefficientPerSecond = .5f;
        private static float trailProcCoefficientPerSecond = 2f;
        private static float trailDamageInterval = 0.25f;
        private static float trailLifetime = 5f;
        private static float trailLingerTime = 5f;
        public static GameObject damageTrailPrefab;


        private static float speedForMaxBreach = 160f;
        private static float breachMinimumY = 0.05f;

        private static float breachAimVelocity = 1.2f;
        private static float breachForwardVelocity = 6f;
        private static float breachUpwardVelocity = 16f;
        private static float breachVelocityAimConversion = 0.55f;

        private static float breachDamageCoefficient = 3f;
        private static float breachProcCoefficient = 1f;
        private static float breachRadius = 9f;
        private static float breachPushForce = 2.5f;
        private static float breachKnockupForce = 6f;
        private static float breachVerticalForce = 18f;
        private static float breachNonJumpExitCoefficient = 0.5f;
        public static GameObject breachEffectPrefab;
        public static GameObject breachImpactEffectPrefab;
        public static SkillDef breachSkillOverride;

        private static string enterSoundString = "Play_acrid_shift_land";
        private static string soundLoopStartEvent = "Play_acrid_shift_fly_loop";
        private static string soundLoopStopEvent = "Stop_acrid_shift_fly_loop";

        private static bool exitOnUngrounded = false;
        private static bool invincibleTEST = true;
        private static float extraReSwimTime = 0.8f;
        private static int maxReSwims = 1;

        public float swimAge;
        public int reSwimCount;
        public float entryVerticalSpeed;
        private Vector3 convertedVelocity;
        private Vector3 currentVelocity;
        private Vector3 finalVelocity;
        private float duration;
        private Run.FixedTimeStamp exitBufferTime;
        private NemCrocoDamageTrail damageTrail;
        private bool hasBreached;
        private bool jumpExit;
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
            swimAge = Mathf.Max(0f, swimAge - extraReSwimTime);

            GetModelTransform().GetComponent<AimAnimator>().enabled = true;
            PlayCrossfade("Gesture, Override", "Swim", 0.1f);
            //PlayCrossfade("Gesture, AdditiveHigh", "Leap", 0.1f); // TODO: Check what this does on acrid

            Util.PlaySound(enterSoundString, gameObject);
            Util.PlaySound(soundLoopStartEvent, gameObject);

            float speedToConvert = Mathf.Min(entryVerticalSpeed, maxVerticalSpeed);
            convertedVelocity = Vector3.down * speedToConvert * verticalSpeedConversion;
            currentVelocity = characterMotor.velocity;
            currentVelocity.y = 0;

            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            gameObject.layer = LayerIndex.GetAppropriateFakeLayerForTeam(teamComponent.teamIndex).intVal;
            characterMotor.Motor.RebuildCollidableLayers();

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

            GetModelTransform().GetComponent<CharacterModel>().invisibilityCount++; /// TEMP. NEED SWIMMING ANIMATION

            if (NetworkServer.active)
            {
                if (invincibleTEST)
                {
                    characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                }
                characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);
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

            swimAge += Time.fixedDeltaTime;
            if (isAuthority)
            {
                ApplyMovement();
                //GatherInputs();

                if (fixedAge >= minimumDuration - exitBufferDuration && (inputBank.jump.justPressed || swimAge >= baseDuration))
                {
                    jumpExit = true;
                }

                bool exitInput = inputBank.skill1.justPressed || inputBank.skill2.justPressed || inputBank.skill3.justPressed || inputBank.skill4.justPressed || inputBank.jump.justPressed;
                if (exitInput)
                {
                    exitBufferTime = Run.FixedTimeStamp.now;
                }
                exitInput |= exitBufferTime.timeSince <= exitBufferDuration;

                if (fixedAge >= minimumDuration && (exitInput || swimAge >= baseDuration))
                {
                    Breach();
                    outer.SetNextStateToMain();
                    return;
                }


                // TODO: BETTER GROUND STICKING, BETTER RE-DIVE CONDITIONS
                if (exitOnUngrounded && !isGrounded)
                {
                    if (!Physics.Raycast(characterBody.footPosition, Vector3.down, maxGroundDistance, LayerIndex.world.intVal, QueryTriggerInteraction.Ignore))
                    {
                        if (reSwimCount >= maxReSwims)
                        {
                            Breach();
                            outer.SetNextStateToMain();
                        }
                        else
                        {
                            outer.SetNextState(new Dive { enteredFromSwim = true, swimAge = this.swimAge, reSwimCount = reSwimCount + 1 });
                        }
                        return;
                    }
                }
            }
        }
        //private void GatherInputs()
        //{
        //    HandleSkill(skillLocator.primary, ref inputBank.skill1);
        //}
        //private void HandleSkill(GenericSkill skillSlot, ref InputBankTest.ButtonState buttonState)
        //{
        //    if (skillSlot && buttonState.down)
        //    {
        //        if (!skillSlot.mustKeyPress || !buttonState.hasPressBeenClaimed)
        //        {
        //            if (skillSlot.ExecuteIfReady())
        //            {
        //                buttonState.hasPressBeenClaimed = true;
        //            }
        //        }
        //    }
        //}
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
            float bonusHorizontalSpeed = new Vector3(convertedVelocity.x, 0f, convertedVelocity.z).magnitude;

            finalVelocity = currentVelocity + bonusHorizontalSpeed * currentVelocity.normalized;

            characterMotor.AddDisplacement(finalVelocity * Time.fixedDeltaTime); //////////////////////////////
            characterDirection.moveVector = finalVelocity;

            // add gravity
            characterMotor.velocity += Physics.gravity * gravityCoefficient * Time.fixedDeltaTime;
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
                //rotation = Util.QuaternionSafeLookRotation(characterMotor.velocity)
            }, false);

            if (isAuthority)
            {
                float aimVelocity = breachAimVelocity;
                float upwardVelocity = breachUpwardVelocity;
                float forwardVelocity = breachForwardVelocity;

                characterBody.isSprinting = true;
                aimVector.y = Mathf.Max(aimVector.y, breachMinimumY);
                Vector3 aimVelocityVector = aimVector.normalized * aimVelocity * moveSpeedStat;
                Vector3 upwardVelocityVector = Vector3.up * upwardVelocity;
                Vector3 forwardVelocityVector = new Vector3(aimVector.x, 0f, aimVector.z).normalized * forwardVelocity;
                Vector3 breachVelocityVector = breachVelocityAimConversion * finalVelocity.magnitude * aimVector.normalized;

                if (jumpExit == false)
                {
                    // if we exit without a jump (i.e use a combat skill), dont leap as far forwards.
                    aimVelocityVector *= breachNonJumpExitCoefficient;
                    breachVelocityVector *= breachNonJumpExitCoefficient;
                }

                characterMotor.Motor.ForceUnground(0.1f);
                characterMotor.velocity = aimVelocityVector + upwardVelocityVector + forwardVelocityVector + breachVelocityVector;

                Vector3 force = Vector3.up * breachKnockupForce + characterMotor.velocity.normalized * breachPushForce;
                DamageTypeCombo damageType = DamageType.Stun1s;
                damageType.damageSource = DamageSource.Utility;
                damageType.AddModdedDamageType(SS2.Survivors.NemCroco.RadiationOnHit);
                var blastAttack = new BlastAttack
                {
                    attacker = gameObject,
                    baseDamage = damageStat * breachDamageCoefficient,
                    baseForce = breachPushForce,
                    bonusForce = force,
                    physForceFlags = PhysForceFlags.ignoreGroundStick | PhysForceFlags.massIsOne,// TODO: SIMPLIFIEDMASS
                    crit = RollCrit(),
                    damageType = damageType,
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
            if (nextState is ChompLeap || nextState is Dive)
            {
                Breach();
            }
        }

        public override void OnExit()
        {
            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

            GetModelTransform().GetComponent<CharacterModel>().invisibilityCount--; ////
            if (NetworkServer.active)
            {
                if (invincibleTEST)
                    characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);

                characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
            }

            gameObject.layer = LayerIndex.GetAppropriateLayerForTeam(teamComponent.teamIndex);
            characterMotor.Motor.RebuildCollidableLayers();

            Util.PlaySound(soundLoopStopEvent, gameObject);

            PlayCrossfade("FullBody, Override", "BufferEmpty", 0.1f);

            if (damageTrail)
            {
                damageTrail.active = false;
                damageTrail.transform.SetParent(null);
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
