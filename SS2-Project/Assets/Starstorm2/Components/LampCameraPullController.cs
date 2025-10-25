using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Audio;
namespace SS2
{
    public class LampCameraPullController : MonoBehaviour
    {
        public float maxDistance = 80f;
        public float cycleInterval = 0.2f;
        public BuffDef buffDef;
        public float buffDuration = 0.3f;

        private BullseyeSearch bullseyeSearch = new BullseyeSearch();
        private List<HurtBox> cycleTargets = new List<HurtBox>();
        private Run.FixedTimeStamp previousCycle = Run.FixedTimeStamp.negativeInfinity;
        private int cycleIndex;

        private TeamComponent teamComponent;
        private LoopSoundDef activeLoopDef;
        private void Awake()
        {
            teamComponent = GetComponent<TeamComponent>();
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                ServerFixedUpdate();
            }
        }

        private void ServerFixedUpdate()
        {
            float t = Mathf.Clamp01(this.previousCycle.timeSince / this.cycleInterval);
            int numCycles = (t == 1f) ? this.cycleTargets.Count : Mathf.FloorToInt(this.cycleTargets.Count * t);
            Vector3 position = base.transform.position;
            for(; this.cycleIndex < numCycles; this.cycleIndex++)
            {
                HurtBox hurtBox = this.cycleTargets[this.cycleIndex];
                if (!hurtBox || !hurtBox.healthComponent)
                {
                    continue;
                }
                CharacterBody body = hurtBox.healthComponent.body;
                if (!CanTarget(body))
                {
                    continue;
                }

                Vector3 corePosition = body.corePosition;
                RaycastHit raycastHit;
                if (!Physics.Linecast(position, corePosition, out raycastHit, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    DebuffBodyServer(body);
                }
            }
            if (this.previousCycle.timeSince >= this.cycleInterval)
            {
                this.previousCycle = Run.FixedTimeStamp.now;
                this.cycleIndex = 0;
                this.cycleTargets.Clear();
                this.SearchForTargets(this.cycleTargets);
            }
        }

        private bool CanTarget(CharacterBody body)
        {
            return true;
        }
        private void SearchForTargets(List<HurtBox> dest)
        {
            bullseyeSearch.searchOrigin = transform.position;
            bullseyeSearch.minAngleFilter = 0f;
            bullseyeSearch.maxAngleFilter = 180f;
            bullseyeSearch.maxDistanceFilter = maxDistance;
            bullseyeSearch.filterByDistinctEntity = true;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
            bullseyeSearch.viewer = null;
            bullseyeSearch.RefreshCandidates();
            //bullseyeSearch.teamMaskFilter = TeamMask.GetUnprotectedTeams(teamComponent.teamIndex);
            dest.AddRange(bullseyeSearch.GetResults());
        }

        private void DebuffBodyServer(CharacterBody body)
        {
            if (body.bodyIndex != Monsters.Lamp.BodyIndex && body.bodyIndex != Monsters.LampBoss.BodyIndex && body.gameObject != gameObject)
            {
                body.AddTimedBuff(this.buffDef, buffDuration);
            }
            if (FriendlyFireManager.ShouldSplashHitProceed(body.healthComponent, teamComponent.teamIndex))
            {
                LampCameraPullAttachment.AddPullToBody(body.gameObject, this, buffDuration);
            }
            
        }
    }
}
