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

        private static float flightDuration = 0.4f;
        private static float maxHeight = 10f;

        private static float leapDistanceIfNoTarget = 10f;

        public HurtBox target;
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
        }

        private Vector3 GetHighestPoint(HurtBox hurtBox)
        {
            return hurtBox.transform.position;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterDirection.forward = characterMotor.velocity;

            if (isAuthority && fixedAge >= flightDuration)
            {
                outer.SetNextState(new Chomp { target = target });
            }
        }
    }
    public class Chomp : BaseSkillState
    {
        private static float baseDuration = .4f;
        private static float damageCoefficient = 1f;
        private static float procCoefficient = 1f;
        private static float pushForce = 300f;
        private static Vector3 bonusForce = Vector3.zero;
        private static float forceMagnitude = 16f;
        private static float hitHopVelocity = 7f;
        private static float hitVelocityMultiplier = 0.2f;
        private static float bloom = 1f;
        private static float recoil = 0f;

        public static GameObject hitEffectPrefab;
        public static GameObject effectPrefab;
        private static string hitboxGroupName = "Bite";
        private static string muzzleString = "MouthMuzzle";
        private static string attackSoundString = "Play_imp_attack";

        private static float attackStartTime = 0.0f;
        private static float attackEndTime = 0.5f;

        public HurtBox target;

        private OverlapAttack attack;
        private Animator modelAnimator;
        private float duration;
        private bool hasAttacked;
        private bool hasHit;
        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;
            modelAnimator = base.GetModelAnimator();
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

            attack.AddModdedDamageType(SS2.Survivors.NemCroco.NemesisPoisonOnHit);
            attack.AddModdedDamageType(SS2.Survivors.NemCroco.NemCrocoExecute);

            if (modelAnimator)
            {
                PlayCrossfade("FullBody, Override", "Bite", "Bite.playbackRate", duration, 0.05f);
            }
            characterBody.SetAimTimer(2f);
            
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float t = fixedAge / duration;
            bool inAttackWindow = t >= attackStartTime && t <= attackEndTime;
            if (inAttackWindow)
            {
                if (!hasAttacked)
                {
                    base.AddRecoil(0.9f * recoil, 1.1f * recoil, -0.1f * recoil, 0.1f * recoil);
                    Util.PlayAttackSpeedSound(attackSoundString, gameObject, attackSpeedStat);
                    EffectManager.SimpleMuzzleFlash(effectPrefab, gameObject, muzzleString, false);
                    hasAttacked = true;
                }

                if (isAuthority)
                {
                    attack.forceVector = transform.forward * forceMagnitude;
                    if (attack.Fire(null))
                    {
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

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }


    }
}
