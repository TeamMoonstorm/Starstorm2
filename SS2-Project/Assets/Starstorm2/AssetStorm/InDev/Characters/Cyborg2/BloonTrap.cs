using System;
using System.Collections.Generic;
using HG;
using UnityEngine;
using RoR2;
using JetBrains.Annotations;
using RoR2.Projectile;
using System.Text;
using R2API;
namespace Moonstorm.Starstorm2.Components
{
    public class BloonTrap : MonoBehaviour
    {
		public float moneyMultiplier;
		public int baseMaxMoney = 25;
        public EntityStateMachine machine;
        public float executeThreshold = .15f;
		public TetherVfxOrigin tetherVfxOrigin;
		public float radius = 14f;
		public float grantEliteBuffRadius = 64f;
		public float eliteBuffDuration = 30f;
		public string contextToken;
		public float maxDuration = 16f;

		private float stopwatch;
		private GameObject owner;
		private SphereSearch sphereSearch;
		private int maxMoney;
        private int currentMoney;
		private TeamFilter teamFilter;

		private List<CharacterBody> suckedBodies;
		private void Awake()
		{
			this.teamFilter = base.GetComponent<TeamFilter>();
			this.sphereSearch = new SphereSearch();

			this.suckedBodies = new List<CharacterBody>();
			
			this.maxMoney = Run.instance.GetDifficultyScaledCost(baseMaxMoney, Run.instance.difficultyCoefficient);
		}
        private void Start()
        {
			this.owner = base.GetComponent<ProjectileController>().owner;
		}
        private void OnEnable()
        {
            GlobalEventManager.onCharacterDeathGlobal += TrySuckingDeath;
        }
		private void OnDisable()
		{
			GlobalEventManager.onCharacterDeathGlobal -= TrySuckingDeath;
		}

		// shittest code all time
		//will suck allies but thats funny
		private void TrySuckingDeath(DamageReport deathReport)
        {
			if((base.transform.position - deathReport.victimBody.footPosition).magnitude <= this.radius)
            {
				TrySuck(deathReport.victimBody);
            }
            
        }

        private void FixedUpdate()
        {
			this.stopwatch += Time.fixedDeltaTime;
			//
			//
			//
			//
			//
			//
			//
			//
			// USE ENTITY STATES FUCKING DIPSHITTTTTTTTTTTTT!!!!!!!!!!!!!!!!!!!
			//
			//
			//
			//
			//
			//
			//
			//
			//
			//
			//
			//

			//TEMPORARY UNTIL INTERACTION WORKS XDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
			if (this.stopwatch >= this.maxDuration || IsFull())
			{
				if(this.owner)
					this.owner.GetComponent<CharacterBody>().master.GiveMoney((uint)this.currentMoney);

				Destroy(base.gameObject);
			}

			List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
			this.SearchForTargets(list);
			int i = 0;
			int count = list.Count;
			while (i < count)
			{
				this.TrySuck(list[i].healthComponent.body);
				i++;
			}
			if (this.tetherVfxOrigin)
			{
				List<Transform> list2 = CollectionPool<Transform, List<Transform>>.RentCollection();
				int j = 0;
				int count2 = list.Count;
				while (j < count2)
				{
					HurtBox hurtBox = list[j];
					if (hurtBox)
					{
						Transform item = hurtBox.transform;
						HealthComponent healthComponent = hurtBox.healthComponent;
						if (healthComponent)
						{
							Transform coreTransform = healthComponent.body.coreTransform;
							if (coreTransform)
							{
								item = coreTransform;
							}
						}
						list2.Add(item);
					}
					j++;
				}
				this.tetherVfxOrigin.SetTetheredTransforms(list2);
				CollectionPool<Transform, List<Transform>>.ReturnCollection(list2);
			}
			CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
		}

		private void SearchForTargets(List<HurtBox> list)
        {
			this.sphereSearch.mask = LayerIndex.entityPrecise.mask;
			this.sphereSearch.origin = this.transform.position;
			this.sphereSearch.radius = this.radius;
			this.sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
			this.sphereSearch.RefreshCandidates();
			this.sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(this.teamFilter.teamIndex));
			this.sphereSearch.OrderCandidatesByDistance();
			this.sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
			this.sphereSearch.GetHurtBoxes(list);
			this.sphereSearch.ClearCandidates();
		}

		private void TrySuck(CharacterBody body)
        {
			if (body.healthComponent.alive && (body.bodyFlags & CharacterBody.BodyFlags.ImmuneToExecutes) > CharacterBody.BodyFlags.None)
				return;
			if(!IsFull() && body.healthComponent.combinedHealthFraction <= this.executeThreshold && !suckedBodies.Contains(body))
            {
				SUCK(body);
				suckedBodies.Add(body);

			}
        }
		public void SUCK(CharacterBody body)
        {
			DeathRewards dosh = body.GetComponent<DeathRewards>();
			if (dosh)
			{
				this.currentMoney += Mathf.FloorToInt(dosh.goldReward * this.moneyMultiplier);
			}

			if(body.healthComponent.alive)
				body.healthComponent.Suicide(owner, base.gameObject, DamageType.Generic);

			if (body.modelLocator && body.modelLocator.modelTransform)
			{
				Destroy(body.modelLocator.modelTransform.gameObject);
				// VFX/ sounD
			}
			
			
			if(body.isElite)
            {
				GameObject radialEffectPrefab = SS2Assets.LoadAsset<GameObject>("RadialEliteBuffEffect", SS2Bundle.Indev);

				TeamMask mask = default(TeamMask);
				mask.AddTeam(this.teamFilter.teamIndex);
				SphereSearch search = new SphereSearch
				{
					mask = LayerIndex.entityPrecise.mask,
					origin = base.transform.position,
					radius = grantEliteBuffRadius
				};

				HurtBox[] hurtBoxes = search.RefreshCandidates().FilterCandidatesByHurtBoxTeam(mask).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();
				Color eliteColor = Color.red;
				for (int k = 0; k < BuffCatalog.eliteBuffIndices.Length; k++)
				{
					BuffIndex buffIndex = BuffCatalog.eliteBuffIndices[k];
					BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);

					eliteColor = buffDef.eliteDef.color;

					if (body.HasBuff(buffIndex))
					{			
						foreach (HurtBox hurtBox in hurtBoxes)
						{
							hurtBox.healthComponent.body.AddTimedBuff(buffIndex, eliteBuffDuration);
	
						}
					}
				}

				EffectData effectData1 = new EffectData
				{
					origin = base.transform.position, //////CHILDLOCATRORRRRRRRRRRRRRRRRR
					scale = this.grantEliteBuffRadius,
					color = eliteColor,
				};
				EffectManager.SpawnEffect(radialEffectPrefab, effectData1, true);
			}
			

			GameObject effectPrefab = SS2Assets.LoadAsset<GameObject>("SuckedOffOrbEffect", SS2Bundle.Indev);
			EffectData effectData2 = new EffectData
			{
				origin = body.corePosition,
				genericFloat = 1f,
				genericUInt = (uint)(body.bodyIndex + 1)
			};
			effectData2.SetNetworkedObjectReference(base.gameObject);
			EffectManager.SpawnEffect(effectPrefab, effectData2, true);
		}

        public bool IsFull()
        {
            return currentMoney >= maxMoney || this.stopwatch >= this.maxDuration;
        }
    }


}
