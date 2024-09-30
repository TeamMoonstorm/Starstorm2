using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Equipments
{
    public sealed class RockFruit : SS2Equipment
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acRockFruit", SS2Bundle.Equipments);

        public override bool Execute(EquipmentSlot slot)
        {
            var allItems = new List<ItemIndex>();
            for (var item = RoR2Content.Items.Syringe.itemIndex; item < (ItemIndex)ItemCatalog.itemCount; item++)
            {
                var def = ItemCatalog.GetItemDef(item);
                if (def.tier != ItemTier.NoTier)
                {
                    if (slot.inventory.GetItemCount(item) > 0)
                    {
                        allItems.Add(item);
                    }
                }
            }

            var randomItem = GetRandomItem(allItems);
            slot.inventory.RemoveItem(randomItem);

            var itemIndex = ItemCatalog.GetItemDef(randomItem).tier;

            List<ItemIndex> list = new List<ItemIndex>();

            var dropList = GetDropListFromItemTier(itemIndex);

            PickupDef itemToGive = null;

            if (dropList != null)
            {
                foreach (var item in dropList)
                {
                    itemToGive = PickupCatalog.GetPickupDef(item);
                    if (itemToGive != null)
                    {
                        list.Add(itemToGive.itemIndex);
                    }
                }
            }

            bool isEmpty = list.Count <= 0;

            if (!isEmpty)
            {
                var newRandomItem = GetRandomItem(list);

                if (slot.inventory && newRandomItem != ItemIndex.None && !isEmpty)
                {
                    slot.inventory.GiveItem(newRandomItem);
                }
            }

            CharacterMasterNotificationQueue.SendTransformNotification(slot.characterBody.master, randomItem, itemToGive.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);

            return true;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        //literally lifted 4 year old moonfall code
        private ItemIndex GetRandomItem(List<ItemIndex> items)
        {
            bool isEmpty = items.Count <= 0;
            if (!isEmpty)
            {
                int itemID = UnityEngine.Random.Range(0, items.Count);
                return items[itemID];
            }
            else
            {
                Debug.Log("Failed to find item!");
                return RoR2Content.Items.Syringe.itemIndex;
            }
        }

        public List<PickupIndex> GetDropListFromItemTier(ItemTier itemTier)
        {
            switch (itemTier)
            {
                case ItemTier.Tier1:
                    return Run.instance.availableTier1DropList;
                case ItemTier.Tier2:
                    return Run.instance.availableTier2DropList;
                case ItemTier.Tier3:
                    return Run.instance.availableTier3DropList;
                case ItemTier.Lunar:
                    return Run.instance.availableLunarItemDropList;
                case ItemTier.Boss:
                    return Run.instance.availableBossDropList;
                case ItemTier.VoidTier1:
                    return Run.instance.availableVoidTier1DropList;
                case ItemTier.VoidTier2:
                    return Run.instance.availableVoidTier2DropList;
                case ItemTier.VoidTier3:
                    return Run.instance.availableVoidTier3DropList;

                default:
                    Debug.Log("Failed to find droplist for " + itemTier);
                    return null;
            }
        }
    }
}
