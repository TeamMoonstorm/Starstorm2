using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.UI;
using SS2.Components;
using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
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
			MimicSpawner.masterObject = _masterPrefab;

			GlobalEventManager.onServerDamageDealt += ServerDamageStealItem;
			On.RoR2.UI.PingIndicator.RebuildPing += RebuildPingOverrideInteractable;
			//On.RoR2.PingerController.AttemptPing += Why;
			//On.RoR2.PingerController.GeneratePingInfo += GenPing;

			On.RoR2.CharacterMaster.Start += StartMimic;

			StealItemDamageType = R2API.DamageAPI.ReserveDamageType();
		}

        private void StartMimic(On.RoR2.CharacterMaster.orig_Start orig, CharacterMaster self)
        {
			SS2Log.Warning("self " + self + " | " + self.name + " | " + self.bodyInstanceObject);
			orig(self);
			SS2Log.Warning("AFTER " + self + " | " + self.name + " | " + self.bodyInstanceObject);
		}

        private bool GenPing(On.RoR2.PingerController.orig_GeneratePingInfo orig, Ray aimRay, GameObject bodyObject, out PingerController.PingInfo result)
        {
			result = new PingerController.PingInfo
			{
				active = true,
				origin = Vector3.zero,
				normal = Vector3.zero,
				targetNetworkIdentity = null
			};
			float num;
			aimRay = CameraRigController.ModifyAimRayIfApplicable(aimRay, bodyObject, out num);
			float maxDistance = 1000f + num;
			RaycastHit raycastHit;
			if (Util.CharacterRaycast(bodyObject, aimRay, out raycastHit, maxDistance, LayerIndex.entityPrecise.mask | LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
			{
				SS2Log.Warning("Enemy Character Ray Cast " + raycastHit);
				HurtBox component = raycastHit.collider.GetComponent<HurtBox>();
				if (component && component.healthComponent)
				{
					CharacterBody body = component.healthComponent.body;
					result.origin = body.corePosition;
					result.normal = Vector3.zero;
					result.targetNetworkIdentity = body.networkIdentity;
					SS2Log.Warning(" Determined Enemy : " + result + " | " + body + " | " + component);
				}
			}
			if (Util.CharacterRaycast(bodyObject, aimRay, out raycastHit, maxDistance, LayerIndex.world.mask | LayerIndex.CommonMasks.characterBodiesOrDefault | LayerIndex.pickups.mask, QueryTriggerInteraction.Collide))
			{
				SS2Log.Warning("Other Character Ray Cast " + raycastHit);
				GameObject gameObject = raycastHit.collider.gameObject;
				NetworkIdentity networkIdentity = gameObject.GetComponentInParent<NetworkIdentity>();
				ForcePingable component2 = gameObject.GetComponent<ForcePingable>();
				SS2Log.Warning("GAme  " + gameObject + " | " + networkIdentity + " | " + component2);
				if (!networkIdentity && (component2 == null || !component2.bypassEntityLocator))
				{
					Transform parent = gameObject.transform.parent;
					EntityLocator entityLocator = parent ? parent.GetComponentInChildren<EntityLocator>() : gameObject.GetComponent<EntityLocator>();
					SS2Log.Warning("bar enable  " + parent + " | " + entityLocator + " | ");
					if (entityLocator)
					{
						gameObject = entityLocator.entity;
						networkIdentity = gameObject.GetComponent<NetworkIdentity>();
					}
				}
				result.origin = raycastHit.point;
				result.normal = raycastHit.normal;
				if (networkIdentity)
				{
					result.targetNetworkIdentity = networkIdentity;
				}
				
			}

			return orig(aimRay, bodyObject, out result);
        }

        private void Why(On.RoR2.PingerController.orig_AttemptPing orig, PingerController self, Ray aimRay, GameObject bodyObject)
        {
			orig(self, aimRay, bodyObject);
        }

        private void RebuildPingOverrideInteractable(On.RoR2.UI.PingIndicator.orig_RebuildPing orig, RoR2.UI.PingIndicator self)
		{
			bool printed = false;
			SS2Log.Warning("Rebuild Ping Start : " + self.gameObject.name);
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
			SS2Log.Warning("pingc : " + self.pingTarget + " | ");
			if (self.pingTarget)
			{
				Debug.LogFormat("Ping target {0}", new object[]
				{
					self.pingTarget
				});
				modelLocator = self.pingTarget.GetComponent<ModelLocator>();
				SS2Log.Warning("pingc : " + modelLocator + " | " + displayNameProvider);
				if (displayNameProvider != null)
				{
					//CharacterBody component = self.pingTarget.GetComponent<CharacterBody>();
					MimicPingCorrecter pingc = self.pingTarget.GetComponent<MimicPingCorrecter>();
					SS2Log.Warning("pingc : " + pingc + " | " + self.pingTarget);
					if (pingc)
					{
						self.pingType = PingIndicator.PingType.Interactable;
						string ownerName = self.GetOwnerName();
						string text = ((MonoBehaviour)displayNameProvider) ? Util.GetBestBodyName(((MonoBehaviour)displayNameProvider).gameObject) : "";
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
				}
			}
            if (!printed)
            {
				orig(self);
			}
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
