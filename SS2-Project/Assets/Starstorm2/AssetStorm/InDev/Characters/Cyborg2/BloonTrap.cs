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
    public class BloonTrap : MonoBehaviour, IInteractable
    {
		private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

		public float moneyMultiplier;
		public int baseMaxMoney = 25;
        public EntityStateMachine machine;
        public float executeThreshold = .15f;
		public TetherVfxOrigin tetherVfxOrigin;
		public float radius = 14f;
		public float grantEliteBuffRadius = 64f;
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
			this.owner = base.GetComponent<ProjectileController>().owner;
			this.maxMoney = Run.instance.GetDifficultyScaledCost(baseMaxMoney, Run.instance.difficultyCoefficient);
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

			GameObject effectPrefab = SS2Assets.LoadAsset<GameObject>("SuckedOffOrbEffect", SS2Bundle.Indev);
			EffectData effectData = new EffectData
			{
				origin = body.corePosition,
				genericFloat = 1f,
				genericUInt = (uint)(body.bodyIndex + 1)
			};
			effectData.SetNetworkedObjectReference(base.gameObject);
			EffectManager.SpawnEffect(effectPrefab, effectData, true);
		}

		private void KillBody(HealthComponent healthComponent)
        {
			float combinedHealth = healthComponent.combinedHealth;
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.damage = healthComponent.combinedHealth;
			damageInfo.position = base.transform.position;
			damageInfo.damageType = DamageType.Generic;
			damageInfo.procCoefficient = 1f;
			damageInfo.attacker = this.owner;
			damageInfo.inflictor = base.gameObject;
			healthComponent.Networkhealth = 0f;
			DamageReport damageReport = new DamageReport(damageInfo, healthComponent, damageInfo.damage, combinedHealth);
			healthComponent.killingDamageType = damageInfo.damageType;
			IOnKilledServerReceiver[] components = base.GetComponents<IOnKilledServerReceiver>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnKilledServer(damageReport);
			}
			GlobalEventManager.instance.OnCharacterDeath(damageReport);
		}
        public bool IsFull()
        {
            return currentMoney >= maxMoney || this.stopwatch >= this.maxDuration;
        }

        public string GetContextString([NotNull] Interactor activator)
        {
			BloonTrap.sharedStringBuilder.Clear();
			BloonTrap.sharedStringBuilder.Append(Language.GetString(this.contextToken));

			BloonTrap.sharedStringBuilder.Append(" <nobr>(");
			CostTypeCatalog.GetCostTypeDef(CostTypeIndex.Money).BuildCostStringStyled(this.currentMoney, BloonTrap.sharedStringBuilder, false, true);
			BloonTrap.sharedStringBuilder.Append(")</nobr>");
			
			return BloonTrap.sharedStringBuilder.ToString();
		}

        public Interactability GetInteractability([NotNull] Interactor activator)
        {
			return IsFull() ? Interactability.Available : Interactability.ConditionsNotMet;
        }

        public void OnInteractionBegin([NotNull] Interactor activator)
        {
			CharacterBody body = activator.GetComponent<CharacterBody>();
			if(body)
            {
				body.master.GiveMoney((uint)this.currentMoney);
				//VFX SOUND ETC

				Destroy(base.gameObject);


            }
        }

        public bool ShouldIgnoreSpherecastForInteractibility([NotNull] Interactor activator)
        {
            return false;
        }

        public bool ShouldShowOnScanner()
        {
            return false;
        }
    }


}
