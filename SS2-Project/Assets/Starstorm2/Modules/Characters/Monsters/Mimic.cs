using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.UI;
using System;
using System.Collections;
using UnityEngine;
using static R2API.DamageAPI;

namespace SS2.Monsters
{
	public sealed class Mimic : SS2Monster
	{
		public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acMimic", SS2Bundle.Indev);

		public static GameObject _masterPrefab;
		public static ModdedDamageType StealItemDamageType { get; private set; }

		public override void Initialize()
		{
			_masterPrefab = AssetCollection.FindAsset<GameObject>("MimicMaster");
			GlobalEventManager.onServerDamageDealt += ServerDamageStealItem;
			On.RoR2.UI.PingIndicator.RebuildPing += RebuildPingOverrideInteractable;
			StealItemDamageType = R2API.DamageAPI.ReserveDamageType();
		}

		private void RebuildPingOverrideInteractable(On.RoR2.UI.PingIndicator.orig_RebuildPing orig, RoR2.UI.PingIndicator self)
		{
			self.pingHighlight.enabled = false;
			self.transform.rotation = Util.QuaternionSafeLookRotation(self.pingNormal);
			self.transform.position = (self.pingTarget ? self.pingTarget.transform.position : self.pingOrigin);
			self.transform.localScale = Vector3.one;
			self.positionIndicator.targetTransform = (self.pingTarget ? self.pingTarget.transform : null);
			self.positionIndicator.defaultPosition = self.transform.position;
			IDisplayNameProvider displayNameProvider = self.pingTarget ? self.pingTarget.GetComponentInParent<IDisplayNameProvider>() : null;
			ModelLocator modelLocator = null;
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
				Debug.LogFormat("Ping target {0}", new object[]
				{
					self.pingTarget
				});
				modelLocator = self.pingTarget.GetComponent<ModelLocator>();
				if (displayNameProvider != null)
				{
					CharacterBody component = self.pingTarget.GetComponent<CharacterBody>();
					//MimicPingCorrecter pingc = self.pingTarget.GetComponent <>
					if (component)
					{
						self.pingType = PingIndicator.PingType.Enemy;
						self.targetTransformToFollow = component.coreTransform;
					}
					else
					{
						self.pingType = PingIndicator.PingType.Interactable;
					}
				}
			}
			string ownerName = self.GetOwnerName();
			string text = ((MonoBehaviour)displayNameProvider) ? Util.GetBestBodyName(((MonoBehaviour)displayNameProvider).gameObject) : "";

			//self.pingText.enabled = true;
			//self.pingText.text = ownerName;
			//
			//			this.pingColor = this.interactablePingColor;
			//			this.pingDuration = this.interactablePingDuration;
			//			this.pingTargetPurchaseInteraction = this.pingTarget.GetComponent<PurchaseInteraction>();
			//			this.halcyonShrine = this.pingTarget.GetComponent<HalcyoniteShrineInteractable>();
			//			Sprite interactableIcon = PingIndicator.GetInteractableIcon(this.pingTarget);
			//			SpriteRenderer component3 = this.interactablePingGameObjects[0].GetComponent<SpriteRenderer>();
			//			ShopTerminalBehavior component4 = this.pingTarget.GetComponent<ShopTerminalBehavior>();
			//			TeleporterInteraction component5 = this.pingTarget.GetComponent<TeleporterInteraction>();
			//			if (component4)
			//			{
			//				PickupIndex pickupIndex = component4.CurrentPickupIndex();
			//				IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
			//				string format = "{0} ({1})";
			//				object arg = text;
			//				object arg2;
			//				if (!component4.pickupIndexIsHidden && component4.pickupDisplay)
			//				{
			//					PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
			//					arg2 = Language.GetString(((pickupDef != null) ? pickupDef.nameToken : null) ?? PickupCatalog.invalidPickupToken);
			//				}
			//				else
			//				{
			//					arg2 = "?";
			//				}
			//				text = string.Format(invariantCulture, format, arg, arg2);
			//			}
			//			else if (component5)
			//			{
			//				this.pingDuration = 30f;
			//				this.pingText.enabled = false;
			//				component5.PingTeleporter(ownerName, this);
			//			}
			//			else if (!this.pingTarget.gameObject.name.Contains("Shrine") && (this.pingTarget.GetComponent<GenericPickupController>() || this.pingTarget.GetComponent<PickupPickerController>()))
			//			{
			//				this.pingDuration = 60f;
			//			}
			//			array = this.interactablePingGameObjects;
			//			for (int i = 0; i < array.Length; i++)
			//			{
			//				array[i].SetActive(true);
			//			}
			//			Renderer componentInChildren;
			//			if (modelLocator)
			//			{
			//				componentInChildren = modelLocator.modelTransform.GetComponentInChildren<Renderer>();
			//			}
			//			else
			//			{
			//				componentInChildren = this.pingTarget.GetComponentInChildren<Renderer>();
			//			}
			//			if (componentInChildren)
			//			{
			//				this.pingHighlight.highlightColor = Highlight.HighlightColor.interactive;
			//				this.pingHighlight.targetRenderer = componentInChildren;
			//				this.pingHighlight.strength = 1f;
			//				this.pingHighlight.isOn = true;
			//				this.pingHighlight.enabled = true;
			//			}
			//			component3.sprite = interactableIcon;
			//			component3.enabled = !component5;
			//			if (this.pingTargetPurchaseInteraction && this.pingTargetPurchaseInteraction.costType != CostTypeIndex.None)
			//			{
			//				PingIndicator.sharedStringBuilder.Clear();
			//				CostTypeDef costTypeDef = CostTypeCatalog.GetCostTypeDef(this.pingTargetPurchaseInteraction.costType);
			//				int num = this.pingTargetPurchaseInteraction.cost;
			//				if (this.pingTargetPurchaseInteraction.costType.Equals(CostTypeIndex.Money) && TeamManager.LongstandingSolitudesInParty() > 0)
			//				{
			//					num = (int)((float)num * TeamManager.GetLongstandingSolitudeItemCostScale());
			//				}
			//				costTypeDef.BuildCostStringStyled(num, PingIndicator.sharedStringBuilder, false, true);
			//				Chat.AddMessage(string.Format(Language.GetString("PLAYER_PING_INTERACTABLE_WITH_COST"), ownerName, text, PingIndicator.sharedStringBuilder.ToString()));
			//			}
			//			else
			//			{
			//				Chat.AddMessage(string.Format(Language.GetString("PLAYER_PING_INTERACTABLE"), ownerName, text));
			//			}
			//this.pingText.color = this.textBaseColor * this.pingColor;
			//this.fixedTimer = this.pingDuration;
		}

		private void ServerDamageStealItem(DamageReport obj)
		{
			if (obj.victimBody && obj.victimBody.inventory && obj.attackerBody && obj.attackerBody.inventory && DamageAPI.HasModdedDamageType(obj.damageInfo, StealItemDamageType))
			{
				var itemList = obj.victimBody.inventory.itemAcquisitionOrder;
				if (itemList.Count > 0)
				{
					int item = UnityEngine.Random.Range(0, itemList.Count);
					obj.attackerBody.inventory.GiveItem(itemList[item]);
					obj.victimBody.inventory.RemoveItem(itemList[item]);
				}
			}
		}

		public override bool IsAvailable(ContentPack contentPack)
		{
			return true;
		}

	}
}
