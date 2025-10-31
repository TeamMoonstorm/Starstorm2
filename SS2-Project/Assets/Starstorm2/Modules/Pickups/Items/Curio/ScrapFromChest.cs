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
            On.RoR2.PurchaseInteraction.OnInteractionBegin += OnInteractionBegin;
        }
        private void OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            int scrap = SS2Util.GetItemCountForPlayers(SS2Content.Items.ScrapFromChest);
            if (self.TryGetComponent(out ChestBehavior chest))
            {
                if (scrap > 0 && Util.CheckRoll(33f + 11 * (scrap - 1)))
                {
                    PickupIndex pickup = chest.dropTable.GenerateDrop(chest.rng);
                    if (pickup != PickupIndex.none)
                    {
                        PickupIndex scrapIndex = PickupCatalog.FindScrapIndexForItemTier(PickupCatalog.GetPickupDef(pickup).itemTier);
                        if (scrapIndex != PickupIndex.none)
                        {
                            //
                            // VFX HERE
                            //
                            chest.dropPickup = scrapIndex;
                            chest.dropCount++;
                        }
                    }
                }

            }
        }

    }
}
