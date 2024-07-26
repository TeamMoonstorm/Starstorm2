using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using RoR2.Projectile;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(TeamFilter))]
    [RequireComponent(typeof(ProjectileTargetComponent))]
    public class ProjectileDirectionalTargetFinderAlt : MonoBehaviour
    {
        [Tooltip("How far ahead the projectile should look to find a target.")]
        public float lookRange;

        [Tooltip("How wide the cone of vision for this projectile is in degrees. Limit is 180.")]
        [Range(0f, 180f)]
        public float lookCone;

        [Tooltip("How long before searching for a target.")]
        public float targetSearchInterval = 0.5f;

        [Tooltip("Will not search for new targets once it has one.")]
        public bool onlySearchIfNoTarget;

        [Tooltip("Allows the target to be lost if it's outside the acceptable range.")]
        public bool allowTargetLoss;

        [Tooltip("If set, targets can only be found when there is a free line of sight.")]
        public bool testLoS;

        [Tooltip("Whether or not airborne characters should be ignored.")]
        public bool ignoreAir;

        [Tooltip("The difference in altitude at which a result will be ignored.")]
        public float flierAltitudeTolerance = float.PositiveInfinity;

        public UnityEvent onNewTargetFound;

        public UnityEvent onTargetLost;

        private new Transform transform;

        private TeamFilter teamFilter;

        private ProjectileTargetComponent targetComponent;

        private float searchTimer;

        private bool hasTarget;

        private bool hadTargetLastUpdate;

        private BullseyeSearch bullseyeSearch;

        private HurtBox lastFoundHurtBox;

        private Transform lastFoundTransform;

        private void Start()
        {
            if (!NetworkServer.active)
            {
                base.enabled = false;
                return;
            }
            bullseyeSearch = new BullseyeSearch();
            teamFilter = GetComponent<TeamFilter>();
            targetComponent = GetComponent<ProjectileTargetComponent>();
            transform = base.transform;
            searchTimer = 0f;
        }

        private void FixedUpdate()
        {
            searchTimer -= Time.fixedDeltaTime;
            if (!(searchTimer <= 0f))
            {
                return;
            }
            searchTimer += targetSearchInterval;
            if (allowTargetLoss && (object)targetComponent.target != null && (object)lastFoundTransform == targetComponent.target && !PassesFilters(lastFoundHurtBox))
            {
                SetTarget(null);
            }
            if (!onlySearchIfNoTarget || targetComponent.target == null)
            {
                SearchForTarget();
            }
            hasTarget = targetComponent.target != null;
            if (hadTargetLastUpdate != hasTarget)
            {
                if (hasTarget)
                {
                    onNewTargetFound?.Invoke();
                }
                else
                {
                    onTargetLost?.Invoke();
                }
            }
            hadTargetLastUpdate = hasTarget;
        }

        private bool PassesFilters(HurtBox result)
        {
            CharacterBody body = result.healthComponent.body;
            if (!body || (ignoreAir && body.isFlying))
            {
                return false;
            }
            if (body.isFlying && !float.IsInfinity(flierAltitudeTolerance) && flierAltitudeTolerance < Mathf.Abs(result.transform.position.y - transform.position.y))
            {
                return false;
            }
            return true;
        }

        private void SearchForTarget()
        {
            bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            bullseyeSearch.teamMaskFilter.RemoveTeam(teamFilter.teamIndex);
            bullseyeSearch.filterByLoS = testLoS;
            bullseyeSearch.searchOrigin = transform.position;
            bullseyeSearch.searchDirection = transform.forward;
            bullseyeSearch.maxDistanceFilter = lookRange;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
            bullseyeSearch.maxAngleFilter = lookCone;
            bullseyeSearch.RefreshCandidates();
            IEnumerable<HurtBox> source = bullseyeSearch.GetResults().Where(PassesFilters);
            SetTarget(source.FirstOrDefault());
        }

        private void SetTarget(HurtBox hurtBox)
        {
            lastFoundHurtBox = hurtBox;
            lastFoundTransform = hurtBox?.transform;
            targetComponent.target = lastFoundTransform;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Transform transform = base.transform;
            Vector3 position = transform.position;
            Gizmos.DrawWireSphere(position, lookRange);
            Gizmos.DrawRay(position, transform.forward * lookRange);
            Gizmos.DrawFrustum(position, lookCone * 2f, lookRange, 0f, 1f);
            if (!float.IsInfinity(flierAltitudeTolerance))
            {
                Gizmos.DrawWireCube(position, new Vector3(lookRange * 2f, flierAltitudeTolerance * 2f, lookRange * 2f));
            }
        }
    }
}
