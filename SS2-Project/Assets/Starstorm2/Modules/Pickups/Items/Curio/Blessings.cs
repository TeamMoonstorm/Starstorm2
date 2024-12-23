using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class Blessings : SS2Item
    {
        // scrap from chests
        // potential from chest
        // elites drop items
        // champions drop items

        // hardened shell
        // bleedout
        // shield gating/shield on kill
        // max hp on kill

        // stack crit on shrine use. convert crit chance to crit dmg
        // dmg to elites ?
        // debuffs deal % health dmg
        // more money from mobs. dmg from money?


        //storm:
        //scrap
        //shieldgate
        //elite dmg

        //fire:
        //champions
        //bleedout
        //money

        //ice:
        //elites
        //shell
        //debuffs

        //lightning:
        //potentials
        //hp
        //crit

        //earth:
        //champion
        //shell
        //elite dmg

        //poison:
        //scrap
        //hp
        //debuffs

        //gold:
        //scrap
        //shell
        //money

        //void:
        //potential
        //shield
        //crit
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync <ExtendedAssetCollection>("acBlessings", SS2Bundle.Items);
        private static GameObject optionPrefab;
        private static BasicPickupDropTable eliteDropTable;
        private static Xoroshiro128Plus eliteDropRng;
        private static BasicPickupDropTable bossDropTable;
        private static Xoroshiro128Plus bossDropRng;
        public override void Initialize()
        {
            optionPrefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion();
            On.RoR2.PurchaseInteraction.OnInteractionBegin += OnChestOpened;
            On.RoR2.ChestBehavior.Roll += ChestRoll;
            IL.RoR2.ChestBehavior.BaseItemDrop += ChestDrop;

            Run.onRunStartGlobal += (run) =>
            {
                eliteDropRng = new Xoroshiro128Plus(run.treasureRng.nextUlong);
                bossDropRng = new Xoroshiro128Plus(run.treasureRng.nextUlong);
            };
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
        }

        

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        // guaranteed items on boss kill. chance for boss item
        // chance for items on elite kill
        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;
            if (damageReport.victimTeamIndex == TeamIndex.Player) return;
            int eliteItems = SS2Util.GetItemCountForPlayers(SS2Content.Items.ItemOnEliteKill);
            int bossItems = SS2Util.GetItemCountForPlayers(SS2Content.Items.ItemOnBossKill);
            if (eliteItems > 0 && damageReport.victimIsElite && Util.CheckRoll(4f + 1f * (eliteItems - 1))) // realizing i have no idea how to use run seeds/rng correctly
            {
                PickupIndex pickupIndex = eliteDropTable.GenerateDrop(eliteDropRng);
                if (pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
                }
            }
            if (bossItems > 0 && damageReport.victimIsChampion)
            {
                PickupIndex pickupIndex = PickupIndex.none;
                if (Util.CheckRoll(1 - Mathf.Pow(0.95f, bossItems)) && damageReport.victimBody.TryGetComponent(out DeathRewards deathRewards))
                {
                    pickupIndex = deathRewards.bossDropTable ? deathRewards.bossDropTable.GenerateDrop(bossDropRng) : (PickupIndex)deathRewards.bossPickup;
                }
                if (pickupIndex == PickupIndex.none)
                    pickupIndex = bossDropTable.GenerateDrop(bossDropRng);
                if (pickupIndex != PickupIndex.none)
                {
                    PickupDropletController.CreatePickupDroplet(pickupIndex, damageReport.victimBody.corePosition, Vector3.up * 20f);
                }
            }
        }

        //lower the tier 1 weight of chests, increasing "rarity"
        private void ChestRoll(On.RoR2.ChestBehavior.orig_Roll orig, ChestBehavior self)
        {
            int option = SS2Util.GetItemCountForPlayers(SS2Content.Items.OptionFromChest);
            float tier1Coefficient = Mathf.Pow(0.85f, option);
            BasicPickupDropTable dropTable = null;
            bool valid = self && self.dropTable && (dropTable = (BasicPickupDropTable)self.dropTable) != null;
            if(!valid)
            {
                orig(self);
                return;            
            }
            dropTable.tier1Weight *= tier1Coefficient; // lol.
            dropTable.GenerateWeightedSelection(Run.instance);
            orig(self);
            dropTable.tier1Weight /= tier1Coefficient;
            dropTable.GenerateWeightedSelection(Run.instance);
        }

        //chance to turn each chest drop into a void potential
        private void ChestDrop(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            // GenericPickupController.CreatePickupInfo createPickupInfo = default(GenericPickupController.CreatePickupInfo);
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloca(3),
                x => x.MatchInitobj<GenericPickupController.CreatePickupInfo>());
            if(b)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<GenericPickupController.CreatePickupInfo, ChestBehavior>>((info, chest) =>
                {
                    int option = SS2Util.GetItemCountForPlayers(SS2Content.Items.OptionFromChest);
                    float chance = 1f - Mathf.Pow(0.7f, option);
                    if(Util.CheckRoll(chance))
                    {
                        info.prefabOverride = optionPrefab;
                        info.pickerOptions = PickupPickerController.GenerateOptionsFromArray(chest.dropTable.GenerateUniqueDrops(3, chest.rng));
                        info.pickupIndex = PickupCatalog.FindPickupIndex(PickupCatalog.GetPickupDef(chest.dropPickup).itemTier); // idk if we need to do this? void potentials use the pickupdef for the tier
                    }
                });
            }
        }

        // chance to add 1 to chest drops, then replace the first drop with scrap
        private void OnChestOpened(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);
            int scrap = SS2Util.GetItemCountForPlayers(SS2Content.Items.ScrapFromChest);         
            if (self.TryGetComponent(out ChestBehavior chest))
            {             
                if(scrap > 0 && Util.CheckRoll(20f + 5 * (scrap - 1)))
                {
                    PickupIndex pickup = chest.dropTable.GenerateDrop(chest.rng);
                    if (pickup != PickupIndex.none)
                    {
                        PickupIndex scrapIndex = PickupCatalog.FindScrapIndexForItemTier(PickupCatalog.GetPickupDef(pickup).itemTier);
                        if (scrapIndex != PickupIndex.none)
                        {
                            chest.dropPickup = scrapIndex;
                            chest.dropCount++;
                        }
                    }
                }
                             
            }
        }

        
    }
}
