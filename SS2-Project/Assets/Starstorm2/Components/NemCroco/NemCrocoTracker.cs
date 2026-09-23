using System;
using UnityEngine;
using RoR2;
using System.Collections.Generic;
namespace SS2.Components
{
    [RequireComponent(typeof(InputBankTest))]
    [RequireComponent(typeof(CharacterBody))]
    [RequireComponent(typeof(TeamComponent))]
    public class NemCrocoTracker : MonoBehaviour, ITargetTracker
    {
        public float maxCandidateDistance = 60f;
        public float maxTrackingDistance = 18f;
        public float maxTrackingAngle = 40f;
        public float trackerUpdateFrequency = 10f;

        public GameObject targetIndicatorPrefab;
        public GameObject radiationIndicatorPrefab;

        private HurtBox trackingTarget;
        private CharacterBody characterBody;
        private TeamComponent teamComponent;
        private InputBankTest inputBank;
        private float trackerUpdateStopwatch;
        private Indicator targetIndicator;
        private List<RadiationIndicator> radiationIndicators;
        private List<HurtBox> radiationCandidates;
        private BullseyeSearch targetSearch = new BullseyeSearch();
        private BullseyeSearch candidateSearch = new BullseyeSearch();
        public void Awake()
        {
            targetIndicator = new Indicator(gameObject, targetIndicatorPrefab);
            radiationIndicators = new List<RadiationIndicator>();
            radiationCandidates = new List<HurtBox>();
        }
        public void Start()
        {
            characterBody = GetComponent<CharacterBody>();
            inputBank = GetComponent<InputBankTest>();
            teamComponent = GetComponent<TeamComponent>();

        }
        public HurtBox GetTrackingTarget()
        {
            return trackingTarget;
        }

        public void OnEnable()
        {
            targetIndicator.active = true;
        }
        public void OnDisable()
        {
            targetIndicator.active = false;
        }
        public void FixedUpdate()
        {
            trackerUpdateStopwatch += Time.fixedDeltaTime;
            if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
            {
                trackerUpdateStopwatch -= 1f / trackerUpdateFrequency;
                Ray aimRay = new Ray(inputBank.aimOrigin, inputBank.aimDirection);

                SearchForCandidates(aimRay);

                SearchForTarget(aimRay);
                targetIndicator.targetTransform = (trackingTarget ? trackingTarget.transform : null);
            }
        }

        public void SearchForCandidates(Ray aimRay)
        {
            TeamMask filter = TeamMask.allButNeutral;
            filter.RemoveTeam(teamComponent.teamIndex);
            candidateSearch.teamMaskFilter = filter;
            candidateSearch.filterByLoS = true;
            candidateSearch.searchOrigin = aimRay.origin;
            candidateSearch.searchDirection = aimRay.direction;
            candidateSearch.sortMode = BullseyeSearch.SortMode.None;
            candidateSearch.maxDistanceFilter = maxCandidateDistance;
            candidateSearch.maxAngleFilter = 180f;
            candidateSearch.RefreshCandidates();
            candidateSearch.FilterOutGameObject(gameObject);
            foreach (HurtBox hurtBox in candidateSearch.GetResults())
            {
                if (!radiationCandidates.Contains(hurtBox) && hurtBox.healthComponent.alive && SS2.Survivors.NemCroco.IsFullRadiation(hurtBox.healthComponent))
                {
                    radiationCandidates.Add(hurtBox);

                    var indicator = new RadiationIndicator(gameObject, radiationIndicatorPrefab);
                    indicator.targetTransform = hurtBox.transform;
                    indicator.active = true;
                    radiationIndicators.Add(indicator);
                }
            }

            for (int i = radiationCandidates.Count - 1; i > 0; i--)
            {
                HurtBox hurtBox = radiationCandidates[i];
                if (!hurtBox || !hurtBox.healthComponent.alive || !SS2.Survivors.NemCroco.IsFullRadiation(hurtBox.healthComponent))
                {
                    radiationCandidates.RemoveAt(i);

                    var indicator = radiationIndicators[i]; // obviously an awful way to do this but im l
                    indicator.active = false;
                    radiationIndicators.RemoveAt(i);
                }
            }
        }

        public void SearchForTarget(Ray aimRay)
        {
            TeamMask filter = TeamMask.allButNeutral;
            filter.RemoveTeam(teamComponent.teamIndex);
            targetSearch.teamMaskFilter = filter;
            targetSearch.filterByLoS = true;
            targetSearch.searchOrigin = aimRay.origin;
            targetSearch.searchDirection = aimRay.direction;
            targetSearch.sortMode = BullseyeSearch.SortMode.Angle;
            targetSearch.maxDistanceFilter = maxTrackingDistance;
            targetSearch.maxAngleFilter = maxTrackingAngle;
            targetSearch.RefreshCandidates();
            targetSearch.FilterOutGameObject(gameObject);
            foreach (HurtBox hurtBox in targetSearch.GetResults())
            {
                if (hurtBox.healthComponent.alive && SS2.Survivors.NemCroco.IsFullRadiation(hurtBox.healthComponent))
                {
                    trackingTarget = hurtBox;
                    break;
                }
            }
        }
        
    }
}
