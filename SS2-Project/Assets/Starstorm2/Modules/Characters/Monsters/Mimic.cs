using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
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
            StealItemDamageType = R2API.DamageAPI.ReserveDamageType();
        }

        private void ServerDamageStealItem(DamageReport obj)
        {
            if (obj.victimBody && obj.victimBody.inventory && obj.attackerBody && obj.attackerBody.inventory && DamageAPI.HasModdedDamageType(obj.damageInfo, StealItemDamageType))
            {
                var itemList = obj.victimBody.inventory.itemAcquisitionOrder;
                int item = UnityEngine.Random.Range(0, itemList.Count);
                obj.attackerBody.inventory.GiveItem(itemList[item]);
                obj.victimBody.inventory.RemoveItem(itemList[item]);
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

    }
}
