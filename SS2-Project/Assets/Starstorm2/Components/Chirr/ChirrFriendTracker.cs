using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
namespace Moonstorm.Starstorm2.Components
{
	// just a bullseyesearch to find someone to use Befriend on. ChirrFriendController handles friend behavior
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

		private SkillLocator skillLocator;
		public bool isScepter
        {
			get => _isScepter;
            set
            {
				if (_isScepter == value) return;
				this._isScepter = value;
				this.UpdateScepter();
			}
        }
		private bool _isScepter;

		private CharacterMaster master;
		public ChirrFriendController friendOwnership;

		public static float maximumFriendHealthFraction = 0.5f;

		public void Awake()
		{
			this.skillLocator = base.GetComponent<SkillLocator>();
			this.indicator = new Indicator(base.gameObject, SS2Assets.LoadAsset<GameObject>("ChirrBefriendIndicator", SS2Bundle.Chirr));
		}
		public void Start()
		{
			this.characterBody = base.GetComponent<CharacterBody>();
			GameObject masterObject = this.characterBody.masterObject; // HAHA
			if (masterObject) this.master = masterObject.GetComponent<CharacterMaster>();
			if(this.master)
            {
				this.friendOwnership = this.master.GetComponent<ChirrFriendController>();
				if(!this.friendOwnership)
                {
					this.friendOwnership = this.master.gameObject.AddComponent<ChirrFriendController>();
                }
				this.friendOwnership.isScepter = this.isScepter;
			}
				
			this.inputBank = base.GetComponent<InputBankTest>();
			this.teamComponent = base.GetComponent<TeamComponent>();
		}
		private void UpdateScepter()
        {
			string asset = this.isScepter ? "ChirrBefriendScepterIndicator" : "ChirrBefriendIndicator";
			this.indicator.visualizerPrefab = SS2Assets.LoadAsset<GameObject>(asset, SS2Bundle.Chirr);
			if(this.friendOwnership)
				this.friendOwnership.isScepter = this.isScepter;
        }

		private bool ShouldShowTracker()
        {
			return this.skillLocator && this.skillLocator.special.CanExecute();
        }

		public bool ShouldSearchForFriend()
        {
			return this.friendOwnership;// && !this.friendOwnership.hasFriend;
        }
		public bool CheckBody(CharacterBody body)
        {
			bool healthRequirement = true;// body.healthComponent.health < body.healthComponent.fullHealth * maximumFriendHealthFraction;
			bool bossRequirement = !body.isBoss; // scepter

			return healthRequirement && bossRequirement;
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
			if (!Util.HasEffectiveAuthority(base.gameObject)) return;
            
			this.trackerUpdateStopwatch += Time.fixedDeltaTime;
			if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
			{
				this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;
				if(this.ShouldSearchForFriend())
                {
					Ray aimRay = new Ray(this.inputBank.aimOrigin, this.inputBank.aimDirection);
					this.SearchForTarget(aimRay);
					this.indicator.targetTransform = (this.trackingTarget && this.ShouldShowTracker() ? this.trackingTarget.transform : null);
				}
				else
                {				
					this.trackingTarget = null;
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
				if(hurtBox.healthComponent && CheckBody(hurtBox.healthComponent.body))
                {					
					this.trackingTarget = hurtBox;
					return;					
                }
            }
			this.trackingTarget = null;
		}

	}
}
