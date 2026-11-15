using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2;
using R2API;

namespace EntityStates.NemCroco
{
    // get head height and position
    // calculate horizontal velocity from set flight duration
    // calculate y velocity needed to reach head (capped) from set flight duration
    public class ChompLeap : BaseSkillState
    {
        private static string enterSoundString = "Play_acrid_shift_jump";
        public static GameObject leapEffectPrefab;

        private static float flightDuration = 0.25f;
        private static float maxHeight = 14f;
        private static float yOffset = -2f;
        private static float minTargetDistance = 2.8f;

        private static float leapDistanceIfNoTarget = 14f;

        public HurtBox target;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
        public override void OnEnter()
        {
            base.OnEnter();

            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                if (TryGetComponent(out SS2.Components.NemCrocoTracker tracker))
                {
                    target = tracker.GetTrackingTarget();
                }

                Vector3 targetPosition = aimRay.GetPoint(leapDistanceIfNoTarget);
                if (target)
                {
                    targetPosition = GetHighestPoint(target);
                }
                else if (Util.CharacterSpherecast(gameObject, aimRay, 2f, out RaycastHit hit, leapDistanceIfNoTarget, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
                {
                    targetPosition = hit.point;
                }

                float verticalDistance = targetPosition.y - characterBody.footPosition.y;
                if (verticalDistance > maxHeight)
                {
                    verticalDistance = maxHeight;
                }
                Vector3 between = targetPosition - characterBody.footPosition;
                float horizontalDistance = new Vector3(between.x, 0, between.z).magnitude;
                horizontalDistance = Mathf.Abs(horizontalDistance);
                float ySpeed = Trajectory.CalculateInitialYSpeed(flightDuration, verticalDistance);
                float hSpeed = Trajectory.CalculateGroundSpeed(flightDuration, horizontalDistance);

                Vector3 direction = between.normalized;
                Vector3 velocity = new Vector3(hSpeed * direction.x, ySpeed, hSpeed * direction.z);

                characterMotor.Motor.ForceUnground();
                characterMotor.velocity = velocity;
            }

            if (leapEffectPrefab)
            {
                Quaternion direction = Util.QuaternionSafeLookRotation(characterMotor.velocity.normalized);
                EffectManager.SimpleEffect(leapEffectPrefab, characterBody.footPosition, direction, false);
            }
            Util.PlaySound(enterSoundString, gameObject);
            PlayCrossfade("FullBody, Override", "ChompLeap", 0.05f);

            characterBody.fakeActorCounter++; // TEMP. NEED TO TRACK THE TARGET BETTER AND NOT JUST SET VELOCITY IN ONENTER
        }
        public override void OnExit()
        {
            characterBody.fakeActorCounter--;

            base.OnExit();
        }
        private Vector3 GetHighestPoint(HurtBox hurtBox)
        {
            return hurtBox.transform.position + Vector3.up * yOffset;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterDirection.moveVector = characterMotor.velocity;

            bool targetInRange = false;
            if (isAuthority && target)
            {
                characterDirection.moveVector = target.transform.position - characterBody.corePosition;
                targetInRange = Vector3.Distance(target.transform.position, characterBody.corePosition) < minTargetDistance;
            }

            if (isAuthority && (targetInRange || fixedAge >= flightDuration))
            {
                outer.SetNextState(new Chomp { target = target });
            }
        }
    }
    public class Chomp : BaseSkillState
    {
        private static float baseDuration = .4f;
        private static float damageCoefficient = 2.4f;
        private static float procCoefficient = 1f;
        private static float pushForce = 300f;
        private static Vector3 bonusForce = Vector3.zero;
        private static float forceMagnitude = 16f;
        private static float hitHopVelocity = 7f;
        private static float hitVelocityMultiplier = 0.4f;
        private static float hitPauseDuration = 0.22f;
        private static float bloom = 3f;
        private static float recoil = 3f;

        public static GameObject hitEffectPrefab;
        public static GameObject effectPrefab;
        public static NetworkSoundEventDef impactSound;
        private static string hitboxGroupName = "Bite";
        private static string muzzleString = "MouthMuzzle";
        private static string attackSoundString = "Play_blindVermin_attack1_bite";

        private static float attackStartTime = 0.1f;
        private static float attackEndTime = 0.5f;

        public HurtBox target;

        private OverlapAttack attack;
        private float hitPauseTimer;
        private Vector3 storedHitPauseVelocity;
        private Animator animator;
        private float duration;
        private bool hasAttacked;
        private bool hasHit;
        private GameObject swingEffectInstance;
        protected EffectManagerHelper _emh_swingEffectInstance = null;

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;
            animator = base.GetModelAnimator();
            Transform modelTransform = base.GetModelTransform();
            attack = new OverlapAttack();
            attack.damageType = DamageTypeCombo.GenericPrimary;
            attack.attacker = gameObject;
            attack.inflictor = gameObject;
            attack.teamIndex = GetTeam();
            attack.damage = damageCoefficient * damageStat;
            attack.procCoefficient = procCoefficient;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.forceVector = bonusForce;
            attack.pushAwayForce = pushForce;
            attack.hitBoxGroup = FindHitBoxGroup(hitboxGroupName);
            attack.isCrit = RollCrit();
            attack.maximumOverlapTargets = 1000;
            if (impactSound) attack.impactSound = impactSound.index;

            attack.AddModdedDamageType(SS2.Survivors.NemCroco.NemesisPoisonOnHit);
            attack.AddModdedDamageType(SS2.Survivors.NemCroco.NemCrocoExecute);

            if (animator)
            {
                PlayCrossfade("FullBody, Override", "Bite", "Bite.playbackRate", duration, 0.05f);
            }
        }

        public override void OnExit()
        {
            if (_emh_swingEffectInstance != null && _emh_swingEffectInstance.OwningPool != null)
            {
                _emh_swingEffectInstance.OwningPool.ReturnObject(_emh_swingEffectInstance);
            }
            else
            {
                if (swingEffectInstance)
                {
                    EntityState.Destroy(swingEffectInstance);
                }
            }

            if (animator)
            {
                animator.speed = 1f;
            }

            base.OnExit();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hasAttacked)
            {
                //characterDirection.forward = characterMotor.velocity;
            }
            else
            {
                StartAimMode(2f);
            }

            if (hitPauseTimer > 0)
            {
                hitPauseTimer -= Time.fixedDeltaTime;
                if (characterMotor)
                {
                    characterMotor.velocity = Vector3.zero;
                }
                fixedAge -= Time.fixedDeltaTime;
                if (hitPauseTimer <= 0)
                {
                    AuthorityExitHitPause();
                }
            }

            float t = fixedAge / duration;
            bool inAttackWindow = t >= attackStartTime && t <= attackEndTime;
            if (inAttackWindow)
            {
                if (!hasAttacked)
                {
                    BeginMeleeAttackEffect();
                    hasAttacked = true;
                }

                if (isAuthority)
                {
                    attack.forceVector = transform.forward * forceMagnitude;
                    if (attack.Fire(null))
                    {
                        AuthorityTriggerHitPause();
                        OnHitEnemyAuthority();
                    }
                }
            }
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void BeginMeleeAttackEffect()
        {
            AddRecoil(-0.4f * recoil, -0.8f * recoil, -0.3f * recoil, 0.3f * recoil);
            Util.PlayAttackSpeedSound(attackSoundString, gameObject, attackSpeedStat);
            if (effectPrefab)
            {
                Transform swingEffectParent = FindModelChild(muzzleString);
                if (swingEffectParent)
                {
                    if (!EffectManager.ShouldUsePooledEffect(effectPrefab))
                    {
                        swingEffectInstance = UnityEngine.Object.Instantiate<GameObject>(effectPrefab, swingEffectParent);
                    }
                    else
                    {
                        _emh_swingEffectInstance = EffectManager.GetAndActivatePooledEffect(effectPrefab, swingEffectParent, true);
                        swingEffectInstance = _emh_swingEffectInstance.gameObject;
                    }
                    ScaleParticleSystemDuration scaleParticleSystemDuration = swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                    if (scaleParticleSystemDuration)
                    {
                        scaleParticleSystemDuration.newDuration = scaleParticleSystemDuration.initialDuration;
                    }
                }
            }
        }

        private void OnHitEnemyAuthority()
        {
            characterBody.AddSpreadBloom(bloom);

            if (!hasHit)
            {
                hasHit = true;
                characterMotor.velocity *= hitVelocityMultiplier;
                SmallHop(characterMotor, hitHopVelocity);

                characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.CrocoRegen.buffIndex, 0.5f);
            }
        }

        protected void AuthorityTriggerHitPause()
        {
            if (characterMotor)
            {
                storedHitPauseVelocity += characterMotor.velocity;
                characterMotor.velocity = Vector3.zero;
            }
            if (animator)
            {
                animator.speed = 0f;
            }
            if (swingEffectInstance)
            {
                ScaleParticleSystemDuration scaleParticleSystemDuration = swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                if (scaleParticleSystemDuration)
                {
                    scaleParticleSystemDuration.newDuration = 20f;
                }
            }
            hitPauseTimer = hitPauseDuration;
        }

        protected void AuthorityExitHitPause()
        {
            hitPauseTimer = 0f;
            
            if (characterMotor)
            {
                characterMotor.velocity = storedHitPauseVelocity * hitVelocityMultiplier;
                storedHitPauseVelocity.y = Mathf.Max(storedHitPauseVelocity.y, hitHopVelocity);
            }
            storedHitPauseVelocity = Vector3.zero;
            if (animator)
            {
                animator.speed = 1f;
            }
            if (swingEffectInstance)
            {
                ScaleParticleSystemDuration scaleParticleSystemDuration = swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                if (scaleParticleSystemDuration)
                {
                    scaleParticleSystemDuration.newDuration = scaleParticleSystemDuration.initialDuration;
                }
            }
        }
    }
}
