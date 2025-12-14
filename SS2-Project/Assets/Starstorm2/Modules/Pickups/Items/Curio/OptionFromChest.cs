using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.ContentManagement;
using SS2.ItemTiers;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace SS2.Items
{
    public sealed class OptionFromChest : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("OptionFromChest", SS2Bundle.Items);
        
        // Buns: Disabling this since it causes a bug in sol haunt preventing people from unlocking Drifter. Look at TODO Below for where bug happens
        public override bool IsAvailable(ContentPack contentPack) => false;

        private static GameObject optionPrefab;
        public override void Initialize()
        {
            optionPrefab = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OptionPickup/OptionPickup.prefab").WaitForCompletion();
            On.RoR2.ChestBehavior.Roll += ChestRoll;
            IL.RoR2.ChestBehavior.BaseItemDrop += BaseItemDrop; 
        }
        //lower the tier 1 weight of chests, increasing "rarity"
        // TODO: This hook is not a clean way to override loot generation.
        // We really ought to roll up our own droptable, akin to FreeChestDropTable,
        // then we won't even need this hook and we can just generate items from
        // our droptable directly in the ILHook below.
        private void ChestRoll(On.RoR2.ChestBehavior.orig_Roll orig, ChestBehavior self)
        {
            int option = SS2Util.GetItemCountForPlayers(SS2Content.Items.OptionFromChest);
            float tier1Coefficient = Mathf.Pow(0.7f, option);
            BasicPickupDropTable dropTable = null;
            // TODO: It seems the bug is happening on this cast to BasicPickupDropTable
            //[Error: Unity Log] InvalidCastException: Specified cast is not valid.
            //Stack trace:
            //SS2.Items.OptionFromChest.ChestRoll(On.RoR2.ChestBehavior + orig_Roll orig, RoR2.ChestBehavior self)(at C:/ Users / color / Desktop / code / Starstorm2 / SS2 - Project / Assets / Starstorm2 / Modules / Pickups / Items / Curio / OptionFromChest.cs:28)
            //(wrapper dynamic - method) MonoMod.Utils.DynamicMethodDefinition.Hook<RoR2.ChestBehavior::Roll> ? -1221049132(RoR2.ChestBehavior)(at IL_0019)
            //RoR2.ChestBehavior.Start()(at<f06ee9a3ef5741e1a8136dd7fb5aa0d7>:IL_004D)
            // Buns: I made the fix Chinchi suggested, but leaving it disabled since I dont have time to test it right now
            bool valid = self && self.dropTable && (self.dropTable as BasicPickupDropTable != null);
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

            // Find where the createPickupInfo is created and put our cursor after it to populate the potential options
            // GenericPickupController.CreatePickupInfo createPickupInfo = default(GenericPickupController.CreatePickupInfo);
            int createPickupInfoVarIndex = -1;
            bool b = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloca(out createPickupInfoVarIndex),
                x => x.MatchInitobj<GenericPickupController.CreatePickupInfo>());
            if (b)
            {
                c.Emit(OpCodes.Ldloc, createPickupInfoVarIndex); // createpickupinfo
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
                        List<UniquePickup> uniquePickups = new List<UniquePickup>();
                        // See the TODO comment above to generate loot from our own custom droptable instead
                        chest.dropTable.GenerateDistinctPickupsPreReplacement(uniquePickups, 3, chest.rng);
                        info.prefabOverride = optionPrefab;
                        info.pickerOptions = PickupPickerController.GenerateOptionsFromList(uniquePickups);
                        info.pickupIndex = PickupCatalog.FindPickupIndex(PickupCatalog.GetPickupDef(chest.dropPickup).itemTier); // idk if we need to do this? void potentials use the pickupdef for the tier
                    }
                    return info;
                });
                c.Emit(OpCodes.Stloc, createPickupInfoVarIndex);
            }
            else
            {
                SS2Log.Fatal("Blessings.BaseItemDrop: ILHook failed to match");
            }
        }

    }
}
