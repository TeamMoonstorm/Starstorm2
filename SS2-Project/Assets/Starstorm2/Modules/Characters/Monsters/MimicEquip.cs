using System.Linq;
using MSU;
using R2API;
using RoR2;
using SS2.Components;
using EntityStates.MimicEquip;
using On.EntityStates.GummyClone;
using RoR2.ContentManagement;
using RoR2.Orbs;
using Starstorm2.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;

namespace SS2.Monsters
{
	public sealed class MimicEquip : SS2Monster
	{
		public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acMimicEquip", SS2Bundle.Indev);

		public static GameObject _masterPrefab;
		public static ModdedDamageType StealItemDamageType { get; private set; }

		public GameObject equipOrb;

		public static GameObject itemStarburst;
	    public static GameObject zipperVFX;
		public static GameObject jetVFX;
		public static GameObject leapLandVFX;
		public static GameObject rechestVFX;

		public override void Initialize()
		{
			_masterPrefab = AssetCollection.FindAsset<GameObject>("MimicEquipMaster");
			
			GlobalEventManager.onServerDamageDealt += ServerDamageStealItem;
			GlobalEventManager.onCharacterDeathGlobal += CharacterDeathGlobalMimicTaunt;

			On.RoR2.Util.GetBestBodyName += GetBestBodyNameRenameMimic;
			On.RoR2.CharacterMaster.Respawn_Vector3_Quaternion_bool += RespawnMimicFixHitboxes;
			On.EntityStates.GummyClone.GummyCloneSpawnState.OnEnter += GummyCloneSpawnStateOnOnEnter;
			
			StealItemDamageType = ReserveDamageType();

			equipOrb = AssetCollection.FindAsset<GameObject>("EquipTakenOrbEffect");
			
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

		private void GummyCloneSpawnStateOnOnEnter(GummyCloneSpawnState.orig_OnEnter orig, EntityStates.GummyClone.GummyCloneSpawnState self)
		{
			orig(self);
			
			if (self.characterBody.baseNameToken == "SS2_MIMIC_EQUIP_BODY_NAME")
			{
				var next = new MimicEquipChestInteractableIdle() { };
				self.outer.SetNextState(next); //leap begin
			}
		}

		private void ServerDamageStealItem(DamageReport obj)
		{
			if (obj.damageInfo.HasModdedDamageType(MimicEquip.StealItemDamageType) && obj.victimBody && obj.victimBody.inventory && obj.attackerBody && obj.attackerBody.inventory)
			{
				EquipmentIndex survEquipIndex = obj.victimBody.inventory.GetEquipmentIndex();
				if (survEquipIndex == EquipmentIndex.None) return;
				
				MimicEquipInventoryManager equipInventoryManager = obj.attackerBody.gameObject.GetComponent<MimicEquipInventoryManager>();
				if (!equipInventoryManager) return;
                
				// i think its more interesting if ther mimic has a random equip it uses .,., .but idk  !
				//obj.attackerBody.inventory.SetEquipmentIndex(itemList, false);
				obj.victimBody.inventory.RemoveEquipment(survEquipIndex);
				equipInventoryManager.AddEquip(survEquipIndex);

				EffectData effectData = new EffectData
				{
					origin = obj.victimBody.corePosition,
					genericFloat = 1.5f,
					genericUInt = (uint)(survEquipIndex + 1)
				};
				effectData.SetNetworkedObjectReference(obj.attacker);
				EffectManager.SpawnEffect(equipOrb, effectData, true);

				PickupDef pickupDef = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(survEquipIndex));
				Chat.SendBroadcastChat(new MimicTheftMessage
				{
					subjectAsCharacterBody = obj.attackerBody,
					baseToken = "SS2_MIMIC_THEFT",
					pickupToken = pickupDef.nameToken,
					pickupColor = pickupDef.baseColor,
					victimName = obj.victimBody.GetDisplayName()
				});
			}
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

        [ConCommand(commandName = "spawn_equipmimic", flags = ConVarFlags.None, helpText = "Spawns a Security Barrel with the specified equipment def.")]
        public static void SpawnEquipMimic(ConCommandArgs args)
        {
	        var spawnCard = SS2Assets.LoadAsset<CharacterSpawnCard>("scMimicEquip", SS2Bundle.Indev);
	        if (spawnCard == null)
	        {
		        spawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
		        spawnCard.prefab = _masterPrefab;
		        spawnCard.sendOverNetwork = true;
		        var body = spawnCard.prefab.GetComponent<CharacterMaster>().bodyPrefab;
	        }
	        var spawnRequest = new DirectorSpawnRequest(
		        spawnCard,
		        new DirectorPlacementRule
		        {
			        placementMode = DirectorPlacementRule.PlacementMode.Direct,
			        position = args.senderBody.footPosition
		        },
		        RoR2Application.rng
	        );
	        spawnRequest.summonerBodyObject = null;
	        spawnRequest.teamIndexOverride = TeamIndex.Monster;
	        spawnRequest.ignoreTeamMemberLimit = true;

	        var masterGameObject = spawnCard.DoSpawn(args.senderBody.footPosition, Quaternion.identity, spawnRequest).spawnedInstance;
	        if (args.TryGetArgInt(0) != null)
	        {
		        masterGameObject.GetComponent<CharacterMaster>().inventory.SetEquipmentIndexForSlot((EquipmentIndex)args.GetArgInt(0), 0);
	        }
	        else
	        {
		        masterGameObject.GetComponent<CharacterMaster>().inventory.SetEquipmentIndexForSlot(EquipmentCatalog.equipmentDefs.FirstOrDefault(def => def.name == args.GetArgString(0)).equipmentIndex, 0);
	        }
        }
        
        public override bool IsAvailable(ContentPack contentPack)
        {
	        return false;
        }
	}
}