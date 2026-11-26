using MSU;
using MSU.Config;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
namespace SS2.Equipments
{
    public sealed class Magnet : SS2Equipment
    {
        private const string token = "SS2_EQUIP_MAGNET_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acMagnet", SS2Bundle.Equipments);


        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Range at which Simple Magnet can pull pickups, in meters.")]
        [FormatToken(token, 0)]
        public static float magnetRadius = 150f;
        public static float pullSpeed = 75f;
        public static float pullDuration = 3f;
        //distance away from player that pickups will land
        public static float destinationRadius = 8f;

        public static int treasurePerPlayer = 1;

        private GameObject _magnetPrefab;
        public override bool Execute(EquipmentSlot slot)
        {
            NetworkServer.Spawn(GameObject.Instantiate<GameObject>(_magnetPrefab, slot.characterBody.corePosition + (Vector3.up * 3f), Quaternion.identity));

            foreach (BuriedTreasureBehavior treasure in InstanceTracker.GetInstancesList<BuriedTreasureBehavior>())
            {
                if (treasure && treasure.IsPointInRadius(slot.transform.position))
                {
                    treasure.SpawnTreasure(slot.transform.position);
                }
            }
            return true;
        }

        
        public override void Initialize()
        {
            _magnetPrefab = AssetCollection.FindAsset<GameObject>("PickupMagnetController");

            var spawnCard = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Chest1/iscChest1.asset").WaitForCompletion();
            AssetCollection.FindAsset<GameObject>("BuriedTreasureZone").GetComponent<BuriedTreasureBehavior>().treasureSpawnCard = spawnCard;

            SceneDirector.onPrePopulateMonstersSceneServer += OnPrePopulateMonstersSceneServer;

        }

        private void OnPrePopulateMonstersSceneServer(SceneDirector sceneDirector)
        {
            if (SceneInfo.instance.countsAsStage && SceneInfo.instance.sceneDef.allowItemsToSpawnObjects)
            {
                Xoroshiro128Plus xoroshiro128Plus = new Xoroshiro128Plus(sceneDirector.rng.nextUlong);
                int treasureCount = 0;
                using (IEnumerator<CharacterMaster> enumerator = CharacterMaster.readOnlyInstancesList.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.inventory.HasEquipment(SS2Content.Equipments.Magnet.equipmentIndex))
                        {
                            treasureCount += treasurePerPlayer;
                        }
                    }
                }
                for (int j = 0; j < treasureCount; j++)
                {
                    DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(SS2Assets.LoadAsset<SpawnCard>("iscBuriedTreasure", SS2Bundle.Equipments), new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Random
                    }, xoroshiro128Plus));
                }
            }
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
    }
}