using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
namespace Moonstorm.Starstorm2.Components
{
	// just a bullseyesearch to find someone to use Befriend on. ChirrFriendOwnership handles friend behavior
	[RequireComponent(typeof(InputBankTest))]
	[RequireComponent(typeof(CharacterBody))]
	[RequireComponent(typeof(TeamComponent))]
	public class ChirrFriendTracker : MonoBehaviour
	{
		public float maxTrackingDistance = 70f;
		public float maxTrackingAngle = 75f;
		public float trackerUpdateFrequency = 10f;
		[NonSerialized]
		public HurtBox trackingTarget;
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

		private CharacterMaster master;
		public ChirrFriendOwnership friendOwnership;

		public static float maximumFriendHealthFraction = 0.5f;

		public void Awake()
		{
			this.indicator = new Indicator(base.gameObject, SS2Assets.LoadAsset<GameObject>("NemMercTrackingIndicator", SS2Bundle.NemMercenary));
		}
		public void Start()
		{
			this.characterBody = base.GetComponent<CharacterBody>();
			GameObject masterObject = this.characterBody.masterObject; // HAHA
			if (masterObject) this.master = masterObject.GetComponent<CharacterMaster>();
			if(this.master)
            {
				this.friendOwnership = this.master.GetComponent<ChirrFriendOwnership>();
				if(!this.friendOwnership)
                {
					this.master.gameObject.AddComponent<ChirrFriendOwnership>();
                }
			}
				
			this.inputBank = base.GetComponent<InputBankTest>();
			this.teamComponent = base.GetComponent<TeamComponent>();

		}

		public bool ShouldSearchForFriend()
        {
			return this.friendOwnership && this.friendOwnership.currentFriend.master != null;
        }
        public HurtBox GetTrackingTarget()
		{
			return this.trackingTarget;
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
			if(Util.HasEffectiveAuthority(base.gameObject))
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
			
		}

		public virtual void SearchForTarget(Ray aimRay)
		{
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
			HurtBox[] hurtBoxes = this.search.GetResults().ToArray();
			foreach(HurtBox hurtBox in hurtBoxes)
            {
				if(hurtBox.healthComponent && hurtBox.healthComponent.alive)
                {
					if(hurtBox.healthComponent.health < hurtBox.healthComponent.fullHealth * maximumFriendHealthFraction)
                    {
						this.trackingTarget = hurtBox;
						return;
					}
                }
            }		
		}
		
	}
}
