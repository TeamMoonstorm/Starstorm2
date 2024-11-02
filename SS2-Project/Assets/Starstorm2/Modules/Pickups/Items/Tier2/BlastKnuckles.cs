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

		public static float blastConeRadius = 3f;
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
					new BulletAttack
					{
						owner = base.gameObject,
						weapon = base.gameObject,
						origin = body.corePosition,
						aimVector = damageDirection,
						maxDistance = blastRange,
						radius = blastConeRadius,
						falloffModel = BulletAttack.FalloffModel.None,
						smartCollision = true,
						stopperMask = 0,
						hitMask = LayerIndex.CommonMasks.bullet,
						damage = body.damage * damage,
						procCoefficient = 1f,
						force = 1600f,
						isCrit = damageInfo.crit,
						hitEffectPrefab = impactEffect,
						damageColorIndex = DamageColorIndex.Item,
					}.Fire();
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
	}
}
