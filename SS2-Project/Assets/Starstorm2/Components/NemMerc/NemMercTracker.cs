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
			this.indicator = new Indicator(base.gameObject, SS2Assets.LoadAsset<GameObject>("NemMercTrackingIndicator", SS2Bundle.NemMercenary));
		}
		public void Start()
		{
			this.characterBody = base.GetComponent<CharacterBody>();
			this.inputBank = base.GetComponent<InputBankTest>();
			this.teamComponent = base.GetComponent<TeamComponent>();

		}
		public virtual GameObject GetTrackingTarget()
		{
			return this.trackingTarget;
		}

		//xd?
		public virtual bool IsTargetHologram()
		{
			return GetTrackingTarget() && this.targetIsHologram;
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

		public virtual void SearchForTarget(Ray aimRay)
		{
			this.targetIsHologram = false;

			RaycastHit[] hits = Physics.SphereCastAll(aimRay.origin, this.hologramRayRadius, aimRay.direction,
				this.hologramTrackingDistance, LayerIndex.noCollision.mask, QueryTriggerInteraction.Collide);

			foreach(RaycastHit hit in hits)
            {

				Collider collider = hit.collider;
				if (collider)
				{
					NemMercHologram hologram = collider.GetComponent<NemMercHologram>();
					// idk why teamfilter is ever null but /shrug
					if (hologram && hologram.teamFilter && hologram.teamFilter.teamIndex == this.teamComponent.teamIndex)
					{
						// TEMPORARY!!!!!!!!!!! I WANT ALLY NEMMERCS TO USE EACHOTHERS HOLOGRAMS
						if(hologram.owner == base.gameObject)
                        {
							//make indicator do animation or smth?
							this.trackingTarget = hologram.gameObject;
							this.targetIsHologram = true;
						}						
						return;
					}
				}
			}

			TeamMask filter = TeamMask.allButNeutral;
			filter.RemoveTeam(this.teamComponent.teamIndex);
			this.search.teamMaskFilter = filter;
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
		[NonSerialized]
		public bool targetIsHologram;

		public float hologramTrackingDistance = 256f;

		public float hologramRayRadius = 6f;

		public float maxTrackingDistance = 50f;

		public float maxTrackingAngle = 40f;

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
