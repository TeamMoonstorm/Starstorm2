using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.CharacterAI;

namespace SS2
{
    [RequireComponent(typeof(BaseAI))]
    public class LampCustomTarget : MonoBehaviour
    {
        public float searchInterval = 1f;

        private BaseAI ai;
        private BullseyeSearch search = new BullseyeSearch();

        private float searchTimer;
        private void Awake()
        {
            ai = GetComponent<BaseAI>();
        }

        private void FixedUpdate()
        {
            if (NetworkServer.active)
            {
                FixedUpdateServer();
            }
        }

        private void FixedUpdateServer()
        {
            searchTimer -= Time.fixedDeltaTime;
            if (searchTimer <= 0)
            {
                searchTimer += searchInterval;
                SearchForTarget();
            }
        }

        private void SearchForTarget()
        {
            if (ai.body)
            {
                Ray aimRay = ai.bodyInputBank.GetAimRay();
                search.viewer = ai.body;
                search.filterByDistinctEntity = true;
                search.filterByLoS = false;
                search.maxDistanceFilter = float.PositiveInfinity;
                search.minDistanceFilter = 0f;
                search.maxAngleFilter = 360f;
                search.searchDirection = aimRay.direction;
                search.searchOrigin = aimRay.origin;
                search.sortMode = BullseyeSearch.SortMode.Distance;
                search.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                search.teamMaskFilter = TeamMask.allButNeutral;
                search.RefreshCandidates();
                search.FilterOutGameObject(ai.body.gameObject);
                var targets = search.GetResults();
                HurtBox bestTarget = null;
                foreach (HurtBox target in targets)
                {
                    if (TargetFilter(target))
                    {
                        // choose the first (closest) target, unless theres a target without LampBuff
                        if (!bestTarget)
                        {
                            bestTarget = target;
                        }
                        if (!target.healthComponent.body.HasBuff(SS2Content.Buffs.bdLampBuff))
                        {
                            bestTarget = target;
                            break;
                        }
                    }
                }
                if (bestTarget)
                {
                    ai.SetCustomTargetGameObject(bestTarget.healthComponent.gameObject);
                }
                
            }
        }

        private bool TargetFilter(HurtBox hurtBox)
        {
            CharacterBody body = hurtBox.healthComponent.body;
            return body.master && body.bodyIndex != Monsters.LampBoss.BodyIndex && body.bodyIndex != Monsters.Lamp.BodyIndex;
        }
    }
}
