using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace SS2.Items
{
    public sealed class OptionFromChest : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("OptionFromChest", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => false;

        private static GameObject optionPrefab;
        public override void Initialize()
        {
            optionPrefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion();
            On.RoR2.ChestBehavior.Roll += ChestRoll;
            IL.RoR2.ChestBehavior.BaseItemDrop += BaseItemDrop; 
        }
        //lower the tier 1 weight of chests, increasing "rarity"
        private void ChestRoll(On.RoR2.ChestBehavior.orig_Roll orig, ChestBehavior self)
        {
            int option = SS2Util.GetItemCountForPlayers(SS2Content.Items.OptionFromChest);
            float tier1Coefficient = Mathf.Pow(0.7f, option);
            BasicPickupDropTable dropTable = null;
            bool valid = self && self.dropTable && (dropTable = (BasicPickupDropTable)self.dropTable) != null;
            if (!valid)
            {
                orig(self);
                return;
            }
            //YES i should replace it with my own drop table. NO im not going to
            dropTable.tier1Weight *= tier1Coefficient; // lol.
            dropTable.tier2Weight /= tier1Coefficient; //????????
            dropTable.tier3Weight /= (tier1Coefficient * 0.7f);
            dropTable.GenerateWeightedSelection(Run.instance);
            orig(self);
            dropTable.tier1Weight /= tier1Coefficient;
            dropTable.tier2Weight *= tier1Coefficient;
            dropTable.tier3Weight *= (tier1Coefficient * 0.7f);
            dropTable.GenerateWeightedSelection(Run.instance);
        }

        //chance to turn each chest drop into a void potential
        private void BaseItemDrop(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            // GenericPickupController.CreatePickupInfo createPickupInfo = default(GenericPickupController.CreatePickupInfo);
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloca(3),
                x => x.MatchInitobj<GenericPickupController.CreatePickupInfo>());
            if (b)
            {
                c.Emit(OpCodes.Ldloc_3); // createpickupinfo
                c.Emit(OpCodes.Ldarg_0); // chestbehavior
                c.EmitDelegate<Func<GenericPickupController.CreatePickupInfo, ChestBehavior, GenericPickupController.CreatePickupInfo>>((info, chest) =>
                {
                    int option = SS2Util.GetItemCountForPlayers(SS2Content.Items.OptionFromChest);
                    float chance = 1f - Mathf.Pow(0.5f, option);
                    if (Util.CheckRoll(chance * 100f))
                    {
                        //
                        // VFX HERE
                        //
                        info.prefabOverride = optionPrefab;
                        info.pickerOptions = PickupPickerController.GenerateOptionsFromArray(chest.dropTable.GenerateUniqueDrops(3, chest.rng));
                        info.pickupIndex = PickupCatalog.FindPickupIndex(PickupCatalog.GetPickupDef(chest.dropPickup).itemTier); // idk if we need to do this? void potentials use the pickupdef for the tier
                    }
                    return info;
                });
                c.Emit(OpCodes.Stloc_3);
            }
            else
            {
                SS2Log.Fatal("Blessings.BaseItemDrop: ILHook failed to match");
            }
        }

    }
}
