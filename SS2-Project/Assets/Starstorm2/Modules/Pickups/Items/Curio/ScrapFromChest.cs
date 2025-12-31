using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;

namespace SS2.Items
{
    public sealed class ScrapFromChest : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("ScrapFromChest", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;

        public override void Initialize()
        {
            PurchaseInteraction.onPurchaseGlobalServer += OnPurchaseGlobalServer;
        }

        private void OnPurchaseGlobalServer(CostTypeDef.PayCostContext payCostContext, CostTypeDef.PayCostResults _)
        {
            int scrap = SS2Util.GetItemCountForPlayers(SS2Content.Items.ScrapFromChest);
            if (payCostContext.purchasedObject.TryGetComponent<ChestBehavior>(out var chest))
            {
                if (scrap > 0 && Util.CheckRoll(33f + 11 * (scrap - 1)))
                {
                    UniquePickup pickup = chest.dropTable.GeneratePickup(chest.rng);
                    if (pickup.pickupIndex != PickupIndex.none)
                    {
                        PickupIndex scrapIndex = PickupCatalog.FindScrapIndexForItemTier(PickupCatalog.GetPickupDef(pickup.pickupIndex).itemTier);
                        if (scrapIndex != PickupIndex.none)
                        {
                            //
                            // VFX HERE
                            //
                            chest.currentPickup = new UniquePickup(scrapIndex);
                            chest.dropCount++;
                        }
                    }
                }
            }
        }
    }
}
