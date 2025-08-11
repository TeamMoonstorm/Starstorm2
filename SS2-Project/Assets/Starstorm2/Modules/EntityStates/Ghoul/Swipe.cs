using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2;

namespace EntityStates.Ghoul
{
    // get head height and position
    // calculate horizontal velocity from set flight duration
    // calculate y velocity needed to reach head (capped) from set flight duration
    //
    public class Leap : BaseGhoulState
    {
        private static float searchDistance = 24f;
        private static float searchAngle = 60f;
        private static string enterSoundString = "Play_imp_attack_tell";
        public static GameObject leapEffectPrefab;

        private static float flightDuration = 0.4f;
        private static float maxHeight = 10f;

        private static float leapDistanceIfNoTarget = 10f;
        public override void OnEnter()
        {
            base.OnEnter();

            if (isAuthority)
            {
                Ray aimRay = GetAimRay();
                BullseyeSearch search = new BullseyeSearch();
                search.maxDistanceFilter = searchDistance;
                search.maxAngleFilter = searchAngle;
                search.searchOrigin = aimRay.origin;
                search.searchDirection = aimRay.direction;
                search.teamMaskFilter = TeamMask.GetEnemyTeams(attackerBody.teamComponent.teamIndex);
                search.sortMode = BullseyeSearch.SortMode.Angle;
                search.viewer = characterBody;
                search.RefreshCandidates();
                search.FilterOutGameObject(gameObject);

                HurtBox target = null;
                foreach (HurtBox hurtBox in search.GetResults())
                {
                    if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.alive)
                    {
                        target = hurtBox;
                        break;
                    }
                }

                Vector3 targetPosition = aimRay.GetPoint(leapDistanceIfNoTarget);
                if (target)
                {
                    targetPosition = GetHighestPoint(target);
                }
                else if (Util.CharacterSpherecast(gameObject, aimRay, 2f, out RaycastHit hit, searchDistance, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
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
        }

        // TODO: gimme head
        private Vector3 GetHighestPoint(HurtBox hurtBox)
        {
            return hurtBox.transform.position;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterDirection.forward = characterMotor.velocity;

            if(isAuthority && fixedAge >= flightDuration)
            {
                outer.SetNextState(new Swipe());
            }
        }
    }
    public class Swipe : BaseGhoulState
    {
        private static float baseDuration = .4f;
        private static float damageCoefficient = 1f;
        private static float forceMagnitude = 16f;
        private static float hitHopVelocity = 7f;
        private static float hitVelocityMultiplier = 0.2f;
        private static float bleedPercentChance = 33f;
        public static GameObject hitEffectPrefab;
        public static GameObject effectPrefab;
        private static string attackSoundString = "Play_imp_attack";

        private static float attackStartTime = 0.0f;
        private static float attackEndTime = .8f;

        private OverlapAttack attack;
        private Animator modelAnimator;
        private float duration;
        private bool hasAttacked;

        private bool isBleed;
        public override void OnEnter()
        {
            base.OnEnter();
            isBleed = Util.CheckRoll(bleedPercentChance, attackerBody.master);

            duration = Swipe.baseDuration / attackSpeedStat; // scales with own attackspeed
            modelAnimator = base.GetModelAnimator();
            Transform modelTransform = base.GetModelTransform();
            attack = new OverlapAttack();
            attack.attacker = attackerObject;
            attack.inflictor = gameObject;
            attack.teamIndex = attackerBody.teamComponent.teamIndex;
            attack.damage = Swipe.damageCoefficient * attackerBody.damage;
            attack.damageType = isBleed ? DamageType.BleedOnHit : DamageType.Generic;
            attack.hitEffectPrefab = Swipe.hitEffectPrefab;
            attack.isCrit = attackerBody.RollCrit();
            

            if (modelTransform)
            {
                attack.hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Swipe");
            }
            if (modelAnimator)
            {
                base.PlayAnimation("Gesture, Override", "Swipe", "Swipe.playbackRate", duration);
            }
            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float t = fixedAge / duration;
            bool inAttackWindow = t >= attackStartTime && t <= attackEndTime;
            if (isAuthority && inAttackWindow)
            {
                if (!hasAttacked)
                {
                    Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
                    EffectManager.SimpleMuzzleFlash(Swipe.effectPrefab, gameObject, "SwipeLeft", true);
                    hasAttacked = true;
                }
                attack.forceVector = transform.forward * Swipe.forceMagnitude;
                if (attack.Fire(null))
                {
                    OnHitEnemyAuthority();
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
            if (characterMotor)
            {
                characterMotor.velocity *= hitVelocityMultiplier;
                SmallHop(characterMotor, hitHopVelocity);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        
    }
}
