using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using SS2;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
    public class BlastKnuckles : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acBlastKnuckles", SS2Bundle.Items);

        private const string TOKEN = "SS2_ITEM_BLASTKNUCKLES_DESC";

		public static int maxCharges = 5;
		public static float cooldownPerCharge = 6f;

		[FormatToken(TOKEN, 3)]
		public static float damageCoefficient = 3f;
		[FormatToken(TOKEN, 4)]
		public static float damageCoefficientPerStack = 2f;
		public static float procCoefficient = 0.5f;

		[FormatToken(TOKEN, 1)]
		public static float blastRadius = 4f;
		[FormatToken(TOKEN, 2)]
		public static float blastRadiusPerStack = 2f;

		[FormatToken(TOKEN, 0)]
		public static float range = 13f;

		private static float sqrRange;
        public override void Initialize()
        {
			sqrRange = Mathf.Pow(range, 2);
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

			private void OnEnable()
			{
				this.indicatorEnabled = true;			
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
			}

            public void OnDamageDealtServer(DamageReport damageReport)
            {
				DamageInfo damageInfo = damageReport.damageInfo;				
				if (damageInfo.damage <= 0) return;

				Vector3 position = damageInfo.position;
				Vector3 between = damageReport.victimBody.corePosition - position;
				if (body.HasBuff(SS2Content.Buffs.BuffBlastKnucklesCharge) && between.sqrMagnitude <= sqrRange)
				{
					float damage = damageCoefficient + damageCoefficientPerStack * (stack - 1);
					float radius = blastRadius + blastRadiusPerStack * (stack - 1);
					EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXQuick"), new EffectData
					{
						origin = position,
						scale = radius,
						rotation = Util.QuaternionSafeLookRotation(damageInfo.force)
					}, true);
					new BlastAttack
					{
						attacker = body.gameObject,
						inflictor = body.gameObject,
						attackerFiltering = AttackerFiltering.Default,
						position = position,
						teamIndex = body.teamComponent.teamIndex,
						radius = radius,
						baseDamage = body.damage * damage,
						damageType = DamageType.Generic,
						crit = damageInfo.crit,
						procCoefficient = .5f,
						procChainMask = default(ProcChainMask),
						baseForce = 300f,
						damageColorIndex = DamageColorIndex.Item,
						falloffModel = BlastAttack.FalloffModel.Linear,
						losType = BlastAttack.LoSType.None,
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
