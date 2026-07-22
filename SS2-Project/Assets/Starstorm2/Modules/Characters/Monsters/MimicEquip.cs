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
using EntityStates.MimicEquip;
using RoR2.Orbs;
using Starstorm2.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static R2API.DamageAPI;
using Object = UnityEngine.Object;

namespace SS2.Monsters
{
	public sealed class MimicEquip : SS2Monster
	{
		public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acMimicEquip", SS2Bundle.Monsters);

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
			_masterPrefab = AssetCollection.FindAsset<GameObject>("MimicEquipMaster");

			GlobalEventManager.onServerDamageDealt += ServerDamageStealItem;

			On.RoR2.Util.GetBestBodyName += GetBestBodyNameRenameMimic;

			On.RoR2.CharacterMaster.Respawn_Vector3_Quaternion_bool += RespawnMimicFixHitboxes;
			GlobalEventManager.onCharacterDeathGlobal += CharacterDeathGlobalMimicTaunt;
			On.RoR2.HealthComponent.TakeDamageProcess += TakeDamagePreventAnnoyingRechest;

			StealItemDamageType = R2API.DamageAPI.ReserveDamageType();

			itemOrb = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ItemTakenOrbEffect.prefab").WaitForCompletion();
			itemOrb.GetComponent<ItemTakenOrbEffect>().enabled = false;
			itemOrb.AddComponent<MimicEquipTakenOrbEffect>();
			itemOrb.GetComponent<MimicEquipTakenOrbEffect>().iconSpriteRenderer = itemOrb.GetComponent<ItemTakenOrbEffect>().iconSpriteRenderer;
			itemOrb.GetComponent<MimicEquipTakenOrbEffect>().particlesToColor = itemOrb.GetComponent<ItemTakenOrbEffect>().particlesToColor;
			itemOrb.GetComponent<MimicEquipTakenOrbEffect>().spritesToColor = itemOrb.GetComponent<ItemTakenOrbEffect>().spritesToColor;
			itemOrb.GetComponent<MimicEquipTakenOrbEffect>().trailToColor = itemOrb.GetComponent<ItemTakenOrbEffect>().trailToColor;
			
			jetVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoDashJets.prefab").WaitForCompletion();
			leapLandVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/CryoCanisterExplosionSecondary.prefab").WaitForCompletion();
			rechestVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/RoboCratePodGroundImpact.prefab").WaitForCompletion();

			itemStarburst = AssetCollection.FindAsset<GameObject>("Chest1Starburst");
			zipperVFX = AssetCollection.FindAsset<GameObject>("ChestUnzipReal");

			var ping = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/ChestIcon_1.png").WaitForCompletion();
			var mid = AssetCollection.FindAsset<InspectDef>("idMimicEquip");
			mid.Info.Visual = ping;

			var commando = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();
			var commandoBank = commando.GetComponent<AkBank>();
			var mimicBank = AssetCollection.bodyPrefab.AddComponent<AkBank>();
			SS2Util.CopyComponent<AkBank>(commandoBank, AssetCollection.bodyPrefab);
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
			var mim = self.GetComponent<MimicEquipInventoryManager>();

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
				var rechest = new MimicEquipChestRechest { taunting = true };
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
					esm.SetNextState(new MimicEquipChestActivateEnter());
				}
            }

			return output;
        }

		//How the mimic steals items, using a custom damage type
		private void ServerDamageStealItem(DamageReport obj)
		{
			if (obj.victimBody && obj.victimBody.inventory && obj.attackerBody && obj.attackerBody.inventory && DamageAPI.HasModdedDamageType(obj.damageInfo, StealItemDamageType))
			{
				EquipmentIndex itemList = obj.victimBody.inventory.GetEquipmentIndex();
				if (itemList == EquipmentIndex.None) return;
				
				var mim = obj.attackerBody.gameObject.GetComponent<MimicEquipInventoryManager>();
				if (!mim) return;
				
				var pdef = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(itemList));

				//obj.attackerBody.inventory.SetEquipmentIndex(itemList, false);
				obj.victimBody.inventory.RemoveEquipment(itemList);
				mim.AddItem(itemList);

				EffectData effectData = new EffectData
				{
					origin = obj.victimBody.corePosition,
					genericFloat = 1.5f,
					genericUInt = (uint)(itemList + 1)
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
			}
		}
	}
}