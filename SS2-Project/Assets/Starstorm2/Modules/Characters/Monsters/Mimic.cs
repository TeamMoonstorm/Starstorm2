using EntityStates.Mimic;
using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.UI;
using SS2.Components;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static R2API.DamageAPI;

namespace SS2.Monsters
{
	public sealed class Mimic : SS2Monster
	{
		public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acMimic", SS2Bundle.Indev);

		public static GameObject _masterPrefab;
		public static ModdedDamageType StealItemDamageType { get; private set; }

		public GameObject itemOrb;

		public static GameObject itemStarburst;
	    public static GameObject zipperVFX;
		public static GameObject jetVFX;
		public static GameObject leapLandVFX;
		public static GameObject rechestVFX;

		public override void Initialize()
		{
			_masterPrefab = AssetCollection.FindAsset<GameObject>("MimicMaster");

			GlobalEventManager.onServerDamageDealt += ServerDamageStealItem;
			On.RoR2.UI.PingIndicator.RebuildPing += RebuildPingOverrideInteractable;
			On.RoR2.CharacterMaster.Respawn += RespawnMimicFixHitboxes;
			GlobalEventManager.onCharacterDeathGlobal += CharacterDeathGlobalMimicTaunt;
			On.RoR2.HealthComponent.TakeDamageProcess += TakeDamagePreventAnnoyingRechest;

			StealItemDamageType = R2API.DamageAPI.ReserveDamageType();

			var ml = AssetCollection.bodyPrefab.GetComponent<ModelLocator>();
			if (ml)
			{
				var transf = ml.modelTransform;
				var componentsInChildren = transf.GetComponentsInChildren<Renderer>();
				foreach (var comp in componentsInChildren)
				{
					comp.material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Chest1/matChest1.mat").WaitForCompletion();
				}
			}

			itemOrb = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ItemTakenOrbEffect.prefab").WaitForCompletion();
			jetVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoDashJets.prefab").WaitForCompletion();
			leapLandVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/CryoCanisterExplosionSecondary.prefab").WaitForCompletion();
			rechestVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePodGroundImpact.prefab").WaitForCompletion();

			itemStarburst = AssetCollection.FindAsset<GameObject>("Chest1Starburst");
			zipperVFX = AssetCollection.FindAsset<GameObject>("ChestUnzipReal");

		    var pip = AssetCollection.FindAsset<GameObject>("MimicBodyNew").GetComponent<PingInfoProvider>();
			var ping = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ChestIcon_1.png").WaitForCompletion();
			pip.pingIconOverride = ping;
			var mid = AssetCollection.FindAsset<InspectDef>("idMimic");
			mid.Info.Visual = ping;

			var commando = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();
			var commandoBank = commando.GetComponent<AkBank>();
			var mimicBank = AssetCollection.bodyPrefab.AddComponent<AkBank>();
			SS2Util.CopyComponent<AkBank>(commandoBank, AssetCollection.bodyPrefab);
		}

		//Along with code in Rechest, prevents mimic from annoyingly rechesting at range when damaged recently.
        private void TakeDamagePreventAnnoyingRechest(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
			orig(self, damageInfo);
			var mim = self.GetComponent<MimicInventoryManager>();

			if (mim)
            {
				mim.rechestPreventionTime = 2.5f;
            }
        }

        //Puts the mimic back into chest mode after it kills someone.
        private void CharacterDeathGlobalMimicTaunt(DamageReport obj)
        {
			if (obj.victimBody && obj.victimBody.isPlayerControlled && obj.attacker && obj.attackerMaster && obj.attackerMaster.masterIndex == MasterCatalog.FindMasterIndex(_masterPrefab))
			{
				var bodyESM = EntityStateMachine.FindByCustomName(obj.attackerMaster.bodyInstanceObject, "Body");
				var rechest = new MimicChestRechest { taunting = true };
				bodyESM.SetNextState(rechest);

				var weaponESM = EntityStateMachine.FindByCustomName(obj.attackerMaster.bodyInstanceObject, "Weapon");
				weaponESM.SetNextStateToMain();
			}
		}

		//Makes it so Respawned mimics via Dios and Void Dios are not invulnerable until purchased.
        private CharacterBody RespawnMimicFixHitboxes(On.RoR2.CharacterMaster.orig_Respawn orig, CharacterMaster self, Vector3 footPosition, Quaternion rotation, bool wasRevivedMidStage)
        {
			var output = orig(self, footPosition, rotation, wasRevivedMidStage);

			if(self.masterIndex == MasterCatalog.FindMasterIndex(_masterPrefab))
            {
				if(self.inventory.GetItemCount(RoR2Content.Items.ExtraLifeConsumed) > 0 || self.inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoidConsumed) > 0)
                {
					var esm = EntityStateMachine.FindByCustomName(self.bodyInstanceObject, "Body");
					esm.SetNextState(new MimicChestActivateEnter());
				}
            }

			return output;
        }

        //Replicating the code of this function until it reaches the point where I can ensure that this IS a mimic, to fix JUST the mimic's ping.
        private void RebuildPingOverrideInteractable(On.RoR2.UI.PingIndicator.orig_RebuildPing orig, RoR2.UI.PingIndicator self)
		{
			bool printed = false;
			self.pingHighlight.enabled = false;
			self.transform.rotation = Util.QuaternionSafeLookRotation(self.pingNormal);
			self.transform.position = (self.pingTarget ? self.pingTarget.transform.position : self.pingOrigin);
			self.transform.localScale = Vector3.one;
			self.positionIndicator.targetTransform = (self.pingTarget ? self.pingTarget.transform : null);
			self.positionIndicator.defaultPosition = self.transform.position;
			IDisplayNameProvider displayNameProvider = self.pingTarget ? self.pingTarget.GetComponentInParent<IDisplayNameProvider>() : null;
			self.pingType = PingIndicator.PingType.Default;
			self.pingObjectScaleCurve.enabled = false;
			self.pingObjectScaleCurve.enabled = true;
			GameObject[] array = self.defaultPingGameObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(false);
			}
			array = self.enemyPingGameObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(false);
			}
			array = self.interactablePingGameObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(false);
			}
			if (self.pingTarget)
			{
				ModelLocator modelLocator = self.pingTarget.GetComponent<ModelLocator>();
				if (displayNameProvider != null)
				{
					MimicPingCorrecter pingc = self.pingTarget.GetComponent<MimicPingCorrecter>();
					if (pingc && pingc.isInteractable)
					{
						self.pingType = PingIndicator.PingType.Interactable;
						string ownerName = self.GetOwnerName();
						var gdnp = self.pingTarget.GetComponent<GenericDisplayNameProvider>();
						
						string text;
						if (gdnp) 
						{
							text = Language.GetString(gdnp.displayToken);
                        }
						else
						{
							text = ((MonoBehaviour)displayNameProvider) ? Util.GetBestBodyName(((MonoBehaviour)displayNameProvider).gameObject) : "";
						}

						self.pingText.enabled = true;
						self.pingText.text = ownerName;

						self.pingColor = self.interactablePingColor;
						self.pingDuration = self.interactablePingDuration;
						self.pingTargetPurchaseInteraction = self.pingTarget.GetComponent<PurchaseInteraction>();
						Sprite interactableIcon = PingIndicator.GetInteractableIcon(self.pingTarget);
						SpriteRenderer component3 = self.interactablePingGameObjects[0].GetComponent<SpriteRenderer>();
						array = self.interactablePingGameObjects;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].SetActive(true);
						}
						Renderer componentInChildren;
						if (modelLocator)
						{
							componentInChildren = modelLocator.modelTransform.GetComponentInChildren<Renderer>();
						}
						else
						{
							componentInChildren = self.pingTarget.GetComponentInChildren<Renderer>();
						}
						if (componentInChildren)
						{
							self.pingHighlight.highlightColor = Highlight.HighlightColor.interactive;
							self.pingHighlight.targetRenderer = componentInChildren;
							self.pingHighlight.strength = 1f;
							self.pingHighlight.isOn = true;
							self.pingHighlight.enabled = true;
						}
						component3.sprite = interactableIcon;
						if (self.pingTargetPurchaseInteraction && self.pingTargetPurchaseInteraction.costType != CostTypeIndex.None)
						{
							PingIndicator.sharedStringBuilder.Clear();
							CostTypeDef costTypeDef = CostTypeCatalog.GetCostTypeDef(self.pingTargetPurchaseInteraction.costType);
							int num = self.pingTargetPurchaseInteraction.cost;
							if (self.pingTargetPurchaseInteraction.costType.Equals(CostTypeIndex.Money) && TeamManager.LongstandingSolitudesInParty() > 0)
							{
								num = (int)((float)num * TeamManager.GetLongstandingSolitudeItemCostScale());
							}
							costTypeDef.BuildCostStringStyled(num, PingIndicator.sharedStringBuilder, false, true);
							Chat.AddMessage(string.Format(Language.GetString("PLAYER_PING_INTERACTABLE_WITH_COST"), ownerName, text, PingIndicator.sharedStringBuilder.ToString()));
						}
						else
						{
							Chat.AddMessage(string.Format(Language.GetString("PLAYER_PING_INTERACTABLE"), ownerName, text));
						}
						self.pingText.color = self.textBaseColor * self.pingColor;
						self.fixedTimer = self.pingDuration;
						printed = true;
                    }
                    else if(pingc && !pingc.isInteractable)
                    {
						string ownerName = self.GetOwnerName();
						string text = ((MonoBehaviour)displayNameProvider) ? Util.GetBestBodyName(((MonoBehaviour)displayNameProvider).gameObject) : "";

						self.pingText.enabled = true;
						self.pingText.text = ownerName;

						self.pingColor = self.enemyPingColor;
						self.pingDuration = self.enemyPingDuration;
						array = self.enemyPingGameObjects;
						for (int i = 0; i < array.Length; i++)
						{
							array[i].SetActive(true);
						}
						if (modelLocator)
						{
							Transform modelTransform = modelLocator.modelTransform;
							if (modelTransform)
							{
								CharacterModel component2 = modelTransform.GetComponent<CharacterModel>();
								if (component2)
								{
									bool flag = false;
									foreach (CharacterModel.RendererInfo rendererInfo in component2.baseRendererInfos)
									{
										if (!rendererInfo.ignoreOverlays && !flag)
										{
											self.pingHighlight.highlightColor = Highlight.HighlightColor.teleporter;
											self.pingHighlight.targetRenderer = rendererInfo.renderer;
											self.pingHighlight.strength = 1f;
											self.pingHighlight.isOn = true;
											self.pingHighlight.enabled = true;
											break;
										}
									}
								}
							}
							Chat.AddMessage(string.Format(Language.GetString("PLAYER_PING_ENEMY"), ownerName, text));
							printed = true;
						}
					}
				}
			}
            if (!printed)
            {
				orig(self);
			}
		}

		//How the mimic steals items, using a custom damage type
		private void ServerDamageStealItem(DamageReport obj)
		{
			if (obj.victimBody && obj.victimBody.inventory && obj.attackerBody && obj.attackerBody.inventory && DamageAPI.HasModdedDamageType(obj.damageInfo, StealItemDamageType))
			{
				var itemList = obj.victimBody.inventory.itemAcquisitionOrder;
				if (itemList.Count > 0)
				{
					var mim = obj.attackerBody.gameObject.GetComponent<MimicInventoryManager>();
					if (mim)
					{
						Util.ShuffleList(itemList);
						for(int i = 0; i < itemList.Count; ++i)
                        {
							var def = ItemCatalog.GetItemDef(itemList[i]);
							
							if(def.tier == ItemTier.NoTier) { continue; }
							
							obj.attackerBody.inventory.GiveItem(def);
							obj.victimBody.inventory.RemoveItem(def);
							mim.AddItem(def.itemIndex);

							EffectData effectData = new EffectData
							{
								origin = obj.victimBody.corePosition,
								genericFloat = 1.5f,
								genericUInt = (uint)(def.itemIndex + 1)
							};
							effectData.SetNetworkedObjectReference(obj.attacker);
							EffectManager.SpawnEffect(itemOrb, effectData, true);
							break;
						}
					}
				}
			}
		}

		public override bool IsAvailable(ContentPack contentPack)
		{
			return true;
		}

	}
}