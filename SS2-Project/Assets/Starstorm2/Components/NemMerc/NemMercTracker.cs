using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;

namespace Moonstorm.Starstorm2.Components
{
	[RequireComponent(typeof(InputBankTest))]
	[RequireComponent(typeof(CharacterBody))]
	[RequireComponent(typeof(TeamComponent))]
	public class NemMercTracker : MonoBehaviour
	{
		public void Awake()
		{
			this.indicator = new Indicator(base.gameObject, LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator"));
		}
		public void Start()
		{
			this.characterBody = base.GetComponent<CharacterBody>();
			this.inputBank = base.GetComponent<InputBankTest>();
			this.teamComponent = base.GetComponent<TeamComponent>();
		}
		public GameObject GetTrackingTarget()
		{
			return this.trackingTarget;
		}

		//xD?
		public bool IsTargetHologram()
        {
			return this.trackingTarget.GetComponent<NemMercHologram>();
        }
		public void OnEnable()
		{
			this.indicator.active = true;
		}
		public void OnDisable()
		{
			this.indicator.active = false;
		}
		public void FixedUpdate()
		{
			this.trackerUpdateStopwatch += Time.fixedDeltaTime;
			if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
			{
				this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;
				Ray aimRay = new Ray(this.inputBank.aimOrigin, this.inputBank.aimDirection);
				this.SearchForTarget(aimRay);
				this.indicator.targetTransform = (this.trackingTarget ? this.trackingTarget.transform : null);
			}
		}

		// prioritize holograms
		public void SearchForTarget(Ray aimRay)
		{
			RaycastHit[] hits = Physics.SphereCastAll(aimRay.origin, this.hologramRayRadius, aimRay.direction,
				this.maxTrackingDistance, LayerIndex.fakeActor.collisionMask, QueryTriggerInteraction.Collide);
			foreach(RaycastHit hit in hits)
            {

				Collider collider = hit.collider;
				if (collider)
				{
					NemMercHologram hologram = collider.GetComponent<NemMercHologram>();
					if (hologram)
					{
						this.trackingTarget = hologram.gameObject;
						return;
					}
				}
			}


			this.search.teamMaskFilter = TeamMask.GetUnprotectedTeams(this.teamComponent.teamIndex);
			this.search.filterByLoS = true;
			this.search.searchOrigin = aimRay.origin;
			this.search.searchDirection = aimRay.direction;
			this.search.sortMode = BullseyeSearch.SortMode.Angle;
			this.search.maxDistanceFilter = this.maxTrackingDistance;
			this.search.maxAngleFilter = this.maxTrackingAngle;
			this.search.RefreshCandidates();
			this.search.FilterOutGameObject(base.gameObject);
			HurtBox hurtBox = this.search.GetResults().FirstOrDefault<HurtBox>();
			this.trackingTarget = hurtBox ? hurtBox.gameObject : null;
		}

		public float hologramRayRadius = 3f;

		public float maxTrackingDistance = 50f;

		public float maxTrackingAngle = 75f;

		public float trackerUpdateFrequency = 10f;

		[NonSerialized]
		public GameObject trackingTarget;

		[NonSerialized]
		public CharacterBody characterBody;

		[NonSerialized]
		public TeamComponent teamComponent;

		[NonSerialized]
		public InputBankTest inputBank;

		[NonSerialized]
		public float trackerUpdateStopwatch;
		[NonSerialized]
		public Indicator indicator;
		[NonSerialized]
		public BullseyeSearch search = new BullseyeSearch();
	}
}
