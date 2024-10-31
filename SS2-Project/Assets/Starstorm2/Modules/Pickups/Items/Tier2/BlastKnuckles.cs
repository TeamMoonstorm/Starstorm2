using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using SS2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
namespace SS2.Items
{
    public class BlastKnuckles : SS2Item
    {
		// TODO: MAKE THE BLAST A CONE FROM PLAYER INSTEAD OF SPHERE ON TARGEWT
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acBlastKnuckles", SS2Bundle.Items);

        private const string TOKEN = "SS2_ITEM_BLASTKNUCKLES_DESC";

		public static int maxCharges = 5;
		public static float cooldownPerCharge = 5f;

		[FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
		public static float damageCoefficient = 4f;
		[FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
		public static float damageCoefficientPerStack = 4f;
		public static float procCoefficient = 0.5f;

		[FormatToken(TOKEN, 0)]
		public static float range = 13f;

		public static float blastAngle = 20f;
		public static float blastRange = 16f;
		private static float sqrRange;

		private static GameObject coneEffect;
		private static GameObject impactEffect;
        public override void Initialize()
        {
			sqrRange = Mathf.Pow(range, 2);
			coneEffect = SS2Assets.LoadAsset<GameObject>("BlastKnucklesCone", SS2Bundle.Items);
			impactEffect = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXQuick");
		}

        public override bool IsAvailable(ContentPack contentPack)
		{
			return true;
		}

		public class BlastKnucklesBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
		{
			[ItemDefAssociation(useOnServer = true, useOnClient = false)]
			private static ItemDef GetItemDef() => SS2Content.Items.BlastKnuckles;
			private GameObject indicatorInstance;
			private float reloadTimer;
			private BullseyeSearch search;
			private SphereSearch sphereSearch;
			private void OnEnable()
			{
				this.indicatorEnabled = true;
				search = new BullseyeSearch();
				sphereSearch = new SphereSearch();
			}

			private void OnDisable()
			{
				this.indicatorEnabled = false;
				body.SetBuffCount(SS2Content.Buffs.BuffBlastKnucklesCharge.buffIndex, 0);			
			}

            private void FixedUpdate()
            {
				if (this.body.GetBuffCount(SS2Content.Buffs.BuffBlastKnucklesCharge) < maxCharges)
				{
					this.reloadTimer += Time.fixedDeltaTime;
					while (this.reloadTimer > cooldownPerCharge && this.body.GetBuffCount(SS2Content.Buffs.BuffBlastKnucklesCharge) < maxCharges)
					{
						this.body.AddBuff(SS2Content.Buffs.BuffBlastKnucklesCharge);
						this.reloadTimer -= cooldownPerCharge;
					}
				}
				else reloadTimer = 0f;
			}

            public void OnDamageDealtServer(DamageReport damageReport)
            {
				DamageInfo damageInfo = damageReport.damageInfo;				
				if (damageInfo.damage <= 0 || !damageInfo.damageType.IsDamageSourceSkillBased) return;

				Vector3 position = damageInfo.position;
				Vector3 between = damageReport.attackerBody.corePosition - position;
				if (body.HasBuff(SS2Content.Buffs.BuffBlastKnucklesCharge) && between.sqrMagnitude <= sqrRange)
				{
					body.RemoveBuff(SS2Content.Buffs.BuffBlastKnucklesCharge);
					float damage = damageCoefficient + damageCoefficientPerStack * (stack - 1);
					Vector3 damageDirection = (position - body.corePosition).normalized;
					EffectManager.SpawnEffect(coneEffect, new EffectData
					{
						origin = body.corePosition,
						scale = 1f,
						rotation = Util.QuaternionSafeLookRotation(damageDirection)
					}, true);
					List<HealthComponent> uniqueTargets = new List<HealthComponent>();

					//cone + small sphere to avoid jank angles
					// i dont really know why i didnt just make this  abulletattack. no one would be able to tell the difference
					search.searchOrigin = body.corePosition;
					search.searchDirection = damageDirection;
					search.teamMaskFilter = TeamMask.all;
					search.teamMaskFilter.RemoveTeam(body.teamComponent.teamIndex);
					search.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
					search.filterByLoS = false;
					search.filterByDistinctEntity = true;
					search.maxAngleFilter = blastAngle;
					search.maxDistanceFilter = blastRange;
					search.RefreshCandidates();
					sphereSearch.radius = 1.5f;
					sphereSearch.origin = body.corePosition;
					sphereSearch.mask = LayerIndex.entityPrecise.mask;
					sphereSearch.RefreshCandidates();
					sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
					
					foreach (HurtBox hurtBox in search.GetResults())
                    {
						if(hurtBox && hurtBox.healthComponent)
                        {
							uniqueTargets.Add(hurtBox.healthComponent);
							DealDamage(hurtBox);
						}
                    }
					foreach(HurtBox hurtBox in sphereSearch.GetHurtBoxes())
                    {
						if (hurtBox && hurtBox.healthComponent && !uniqueTargets.Contains(hurtBox.healthComponent))
							DealDamage(hurtBox);
                    }

					void DealDamage(HurtBox hurtBox)
                    {
						stupidfuck attack = new stupidfuck();
						attack.damage = body.damage * damage;
						attack.attacker = body.gameObject;
						attack.inflictor = body.gameObject;
						attack.force = damageDirection * 1600f;
						attack.crit = damageInfo.crit;
						attack.procChainMask = default(ProcChainMask);
						attack.procCoefficient = 1f;
						attack.position = hurtBox.transform.position;
						attack.damageColorIndex = DamageColorIndex.Item;
						attack.damageType = DamageType.Generic;
						RoR2.Orbs.OrbManager.instance.AddOrb(attack);
						EffectManager.SpawnEffect(impactEffect, new EffectData
						{
							origin = position,
							scale = 1f,
							rotation = Util.QuaternionSafeLookRotation(damageInfo.force)
						}, true);											
					}
				}
			}

            private bool indicatorEnabled
			{
				get
				{
					return this.indicatorInstance;
				}
				set
				{
					if (this.indicatorEnabled == value)
					{
						return;
					}
					if (value)
					{
						GameObject original = SS2Assets.LoadAsset<GameObject>("BlastKnucklesIndicator", SS2Bundle.Items);
						this.indicatorInstance = UnityEngine.Object.Instantiate<GameObject>(original, base.body.corePosition, Quaternion.identity);
						this.indicatorInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject, null);
						return;
					}
					UnityEngine.Object.Destroy(this.indicatorInstance);
					this.indicatorInstance = null;
				}
			}
			
		}
		private class stupidfuck : RoR2.Orbs.Orb
		{
			public override void Begin()
			{
				base.duration = 0;
			}
			public override void OnArrival()
			{
				if (this.target)
				{
					HealthComponent healthComponent = this.target.healthComponent;
					if (healthComponent && healthComponent.alive)
					{
						DamageInfo damageInfo = new DamageInfo();
						damageInfo.damage = damage;
						damageInfo.attacker = attacker;
						damageInfo.inflictor = inflictor;
						damageInfo.force = force;
						damageInfo.crit = crit;
						damageInfo.procChainMask = procChainMask;
						damageInfo.procCoefficient = procCoefficient;
						damageInfo.position = position;
						damageInfo.damageColorIndex = damageColorIndex;
						damageInfo.damageType = damageType;
						healthComponent.TakeDamage(damageInfo);
						GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
						GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
					}
				}
			}
			public float damage;
			public GameObject attacker;
			public GameObject inflictor;
			public Vector3 force;
			public bool crit;
			public ProcChainMask procChainMask;
			public float procCoefficient;
			public Vector3 position;
			public DamageColorIndex damageColorIndex;
			public DamageTypeCombo damageType;
			public TeamIndex teamIndex;
		}

	}
}
