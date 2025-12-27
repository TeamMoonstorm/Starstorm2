using EntityStates.Mimic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MSU;
using R2API;
using R2API.Utils;
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
		public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acMimic", SS2Bundle.Monsters);

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

			IL.RoR2.UI.PingIndicator.RebuildPing += RebuildPingOverrideInteractable;
			On.RoR2.Util.GetBestBodyName += GetBestBodyNameRenameMimic;

			On.RoR2.CharacterMaster.Respawn_Vector3_Quaternion_bool += RespawnMimicFixHitboxes;
			GlobalEventManager.onCharacterDeathGlobal += CharacterDeathGlobalMimicTaunt;
			On.RoR2.HealthComponent.TakeDamageProcess += TakeDamagePreventAnnoyingRechest;

			StealItemDamageType = R2API.DamageAPI.ReserveDamageType();

			var material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Chest1/matChest1.mat").WaitForCompletion();
			var ml = AssetCollection.bodyPrefab.GetComponent<ModelLocator>();
			if (ml)
			{
				var transf = ml.modelTransform;
				var componentsInChildren = transf.GetComponentsInChildren<Renderer>();
				foreach (var comp in componentsInChildren)
				{
					comp.material = material;
				}
			}

			var skd = AssetCollection.FindAsset<SkinDef>("sdMimic");
            if (skd)
            {
				for(int i = 0; i < skd.skinDefParams.rendererInfos.Length; ++i)
                {
					skd.skinDefParams.rendererInfos[i].defaultMaterial = material;
				}
            }

			itemOrb = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ItemTakenOrbEffect.prefab").WaitForCompletion();
			jetVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoDashJets.prefab").WaitForCompletion();
			leapLandVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/CryoCanisterExplosionSecondary.prefab").WaitForCompletion();
			rechestVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePodGroundImpact.prefab").WaitForCompletion();

			itemStarburst = AssetCollection.FindAsset<GameObject>("Chest1Starburst");
			zipperVFX = AssetCollection.FindAsset<GameObject>("ChestUnzipReal");

			var ping = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ChestIcon_1.png").WaitForCompletion();
			var mid = AssetCollection.FindAsset<InspectDef>("idMimic");
			mid.Info.Visual = ping;

			var commando = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();
			var commandoBank = commando.GetComponent<AkBank>();
			var mimicBank = AssetCollection.bodyPrefab.AddComponent<AkBank>();
			SS2Util.CopyComponent<AkBank>(commandoBank, AssetCollection.bodyPrefab);

			ChatMessageBase.chatMessageTypeToIndex.Add(typeof(MimicTheftMessage), (byte)ChatMessageBase.chatMessageIndexToType.Count);
			ChatMessageBase.chatMessageIndexToType.Add(typeof(MimicTheftMessage));

		}

        private string GetBestBodyNameRenameMimic(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        {
            if (bodyObject && bodyObject.TryGetComponent<MimicPingCorrecter>(out var mpc) && mpc.isInteractable)
			{
				if(bodyObject.TryGetComponent<GenericDisplayNameProvider>(out var gdnp)){
					return gdnp.GetDisplayName();
				}
            }
			return orig(bodyObject);
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
        private CharacterBody RespawnMimicFixHitboxes(On.RoR2.CharacterMaster.orig_Respawn_Vector3_Quaternion_bool orig, CharacterMaster self, Vector3 footPosition, Quaternion rotation, bool wasRevivedMidStage)
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

		private void RebuildPingOverrideInteractable(ILContext il)
		{
			ILCursor c = new(il);

			// Locate the variable index for displayNameProvider when it's stored for the first time
			// displayNameProvider = pingTarget.GetComponentInParent<IDisplayNameProvider>()
			int displayProviderVarIndex = -1;
			if (!c.TryGotoNext(
					x => x.MatchCallOrCallvirt(typeof(PingIndicator).GetPropertyGetter(nameof(PingIndicator.pingTarget))),
					x => x.MatchCallOrCallvirt<GameObject>(nameof(GameObject.GetComponentInParent)))
				|| !c.TryGotoNext(
					x => x.MatchStloc(out displayProviderVarIndex)))
			{
				SS2Log.Fatal($"Failed IL Hook {il.Method.Name} #1");
				return;
			}

            // Locate the code section that does
            // if (displayNameProvider != null)
            // {
            //     DoStuff();
            // }

            ILLabel afterDisplayNameProviderNullCheck = null;
			if (!c.TryGotoNext(
					MoveType.After,
					x => x.MatchLdloc(displayProviderVarIndex),
					x => x.MatchBrfalse(out afterDisplayNameProviderNullCheck)))
			{
				SS2Log.Fatal($"Failed IL Hook {il.Method.Name} #2");
				return;
			}

            // And turn it into
            // if (displayNameProvider != null)
            // {
            //     if (pingTarget.TryGetComponent<MimicController>(out var controller))
            //     {
            //         DoOurStuff();
            //     }
            //     else DoStuff();
            // }


            c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<PingIndicator, bool>>(self =>
			{
				if (self.pingTarget.TryGetComponent<MimicPingCorrecter>(out var controller))
				{
					if (controller.isInteractable)
					{
						self.pingType = PingIndicator.PingType.Interactable;
						self.targetTransformToFollow = self.pingTarget.transform;
					}
					else
					{
						self.pingType = PingIndicator.PingType.Enemy;
						self.targetTransformToFollow = self.pingTarget.GetComponent<CharacterBody>().coreTransform;
					}
					return true;
				}
				return false;
			});
			c.Emit(OpCodes.Brtrue, afterDisplayNameProviderNullCheck);
		}


		// Helper method to prevent mimic from stealing items we dont want AI having
		private bool CheckItemTags(ItemDef item)
		{
			return item.ContainsTag(ItemTag.ExtractorUnitBlacklist) || item.ContainsTag(ItemTag.BrotherBlacklist) || item.ContainsTag(ItemTag.CannotSteal);

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
							var itemToStealDef = ItemCatalog.GetItemDef(itemList[i]);
							
							if (itemToStealDef.tier == ItemTier.NoTier || CheckItemTags(itemToStealDef)) 
							{ 
								continue; 
							}

							var pdef = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(itemList[i]));

							obj.attackerBody.inventory.GiveItemPermanent(itemToStealDef);
							obj.victimBody.inventory.RemoveItemPermanent(itemToStealDef);
							mim.AddItem(itemToStealDef.itemIndex);

							EffectData effectData = new EffectData
							{
								origin = obj.victimBody.corePosition,
								genericFloat = 1.5f,
								genericUInt = (uint)(itemToStealDef.itemIndex + 1)
							};
							effectData.SetNetworkedObjectReference(obj.attacker);
							EffectManager.SpawnEffect(itemOrb, effectData, true);

							//"MONSTER_PICKUP": "<style=cWorldEvent>{0} picked up {1}{2}</color>",
							Chat.SendBroadcastChat(new MimicTheftMessage
							{
								subjectAsCharacterBody = obj.attackerBody,
								baseToken = "SS2_MIMIC_THEFT",
								pickupToken = pdef.nameToken,
								pickupColor = pdef.baseColor,
								victimName = obj.victimBody.GetDisplayName()
							});
							break;
						}
					}
				}
			}
		}
	}

	public class MimicTheftMessage : SubjectChatMessage
	{
		public override string ConstructChatString()
		{
			string subjectName = base.GetSubjectName();
			string @string = Language.GetString(base.GetResolvedToken());

			string text = Language.GetString(this.pickupToken) ?? "???";
			text = Util.GenerateColoredString(text, this.pickupColor);
			try
			{
				return string.Format(@string, subjectName, text, victimName);
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
			return "";
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(this.pickupToken);
			writer.Write(this.pickupColor);
			writer.Write(this.victimName);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			this.pickupToken = reader.ReadString();
			this.pickupColor = reader.ReadColor32();
			this.victimName = reader.ReadString();
		}

		public string pickupToken;

		public Color32 pickupColor;

		public string victimName;
	}
}