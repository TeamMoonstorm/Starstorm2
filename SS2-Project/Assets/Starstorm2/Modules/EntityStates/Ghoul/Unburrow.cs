using System;
using RoR2;
using UnityEngine;
using SS2;

namespace EntityStates.Ghoul
{
    public class Unburrow : BaseGhoulState
    {
        // get head height and position
        // calculate horizontal velocity from set flight duration
        // calculate y velocity needed to reach head (capped) from set flight duration
        //
        private static float searchDistance = 40f;
        private static float searchAngle = 30f;
        private static string enterSoundString = "Play_imp_spawn";
        public static GameObject leapEffectPrefab;

        private static float flightDuration = 0.8f;

        private static float leapDistanceIfNoTarget = 15f;

        private bool hasTarget;
        public bool useLeaderAimRay = true;
        public override void OnEnter()
        {
            base.OnEnter();

            if (isAuthority)
            {
                Ray aimRay = useLeaderAimRay ? attackerBody.inputBank.GetAimRay() : base.GetAimRay();
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
                search.FilterOutGameObject(attackerObject);

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
                    hasTarget = true;
                    targetPosition = GetHighestPoint(target);
                }
                else if (Util.CharacterSpherecast(gameObject, aimRay, 2f, out RaycastHit hit, searchDistance, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
                {
                    targetPosition = hit.point;
                }


                float verticalDistance = targetPosition.y - characterBody.footPosition.y;
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

            // play animation
            Util.PlaySound(enterSoundString, gameObject);
            StartAimMode(2f);
        }

        // TODO: gimme head
        private Vector3 GetHighestPoint(HurtBox hurtBox)
        {
            return hurtBox.transform.position;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            StartAimMode(2f);

            if(isAuthority)
            {
                if (characterMotor)
                {
                    // if wasnt on ground last frame, and is on ground now
                    if (!characterMotor.Motor.LastGroundingStatus.IsStableOnGround && characterMotor.Motor.GroundingStatus.IsStableOnGround)
                    {
                        outer.SetNextStateToMain();
                        return;
                    }
                }

                if (fixedAge >= flightDuration)
                {
                    if (hasTarget)
                    {
                        outer.SetNextState(new Swipe());
                    }
                }
            }
            

            
        }
    }
}