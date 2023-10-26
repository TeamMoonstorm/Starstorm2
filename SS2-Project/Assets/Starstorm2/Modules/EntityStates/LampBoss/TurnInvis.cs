using EntityStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Navigation;
using System.Linq;

namespace EntityStates.LampBoss
{
    public class TurnInvis : BaseState
    {
        public static float baseDuration;
        private float duration;
        private bool hasBuffed;
        private Animator animator;
        public static string mecanimParameter;
        private float timer;
        public float blinkDistance = 18f;
        private Vector3 blinkDestination = Vector3.zero;

        public override void OnEnter()
        {
            base.OnEnter();
            hasBuffed = false;
            animator = GetModelAnimator();
            duration = baseDuration / attackSpeedStat;

            CalculateBlinkDestination();

            PlayCrossfade("FullBody, Override", "SecondaryBuff", "Secondary.playbackRate", duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (animator.GetFloat(mecanimParameter) >= 0.5f && !hasBuffed)
            {
                hasBuffed = true;
                SetPosition(blinkDestination);
                characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.Cloak.buffIndex, 8f);
                characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.CloakSpeed.buffIndex, 8f);
            }

            if (fixedAge >= duration)
                outer.SetNextStateToMain();
        }

        private void SetPosition(Vector3 newPosition)
        {
            if (characterMotor)
            {
                characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
            }
        }

        private void CalculateBlinkDestination()
        {
            Vector3 vector = Vector3.zero;
            Ray aimRay = GetAimRay();
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
            bullseyeSearch.maxDistanceFilter = blinkDistance;
            bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            bullseyeSearch.filterByLoS = false;
            bullseyeSearch.teamMaskFilter.RemoveTeam(TeamIndex.Player);
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
            bullseyeSearch.RefreshCandidates();
            HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
            if (hurtBox)
            {
                vector = hurtBox.transform.position - transform.position;
            }
            blinkDestination = transform.position;
            NodeGraph groundNodes = SceneInfo.instance.groundNodes;
            NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(transform.position + vector, characterBody.hullClassification, float.PositiveInfinity);
            groundNodes.GetNodePosition(nodeIndex, out blinkDestination);
            blinkDestination += transform.position - characterBody.footPosition;
            characterDirection.forward = vector;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
