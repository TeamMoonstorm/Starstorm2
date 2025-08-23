using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Items
{
    public sealed class LuckyPup : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acLuckuPup", SS2Bundle.Indev);

        public static float percentPupChange = 10f;

        public override void Initialize()
        {
            SS2Log.Info($"Happy Chilean Independence Day to about {percentPupChange}% of you!");
            CharacterMaster.onCharacterMasterDiscovered += GivePup;
            On.RoR2.CharacterModel.Start += CharacterModel_Start;
            On.RoR2.Util.CheckRoll_float_float_CharacterMaster += IncreaseChance;
        }

        //Clay monger's appearance are based off "Chanchitos", which are three legged clay pigs from a small town on chile known to be good luck charms, ergo, during the single week of independence day players get slight increases to all the chance effects.
        private bool IncreaseChance(On.RoR2.Util.orig_CheckRoll_float_float_CharacterMaster orig, float percentChance, float luck, CharacterMaster effectOriginMaster)
        {
            if(effectOriginMaster.inventory.GetItemCount(SS2Content.Items.LuckyPup) > 0)
            {
                var newPercentChance = percentChance;
                newPercentChance += percentChance / 10;
                return orig(newPercentChance, luck, effectOriginMaster);
            }
            return orig(percentChance, luck, effectOriginMaster);
        }

        // always enable outside of runs
        // what could possibly go wrong?
        private static void CharacterModel_Start(On.RoR2.CharacterModel.orig_Start orig, CharacterModel self)
        {
            orig(self);
            if (!Run.instance)
            {
                self.EnableItemDisplay(SS2Content.Items.LuckyPup.itemIndex);
            }
        }

        private void GivePup(CharacterMaster obj)
        {
            if (!NetworkServer.active)
                return;

            if(Util.CheckRoll(percentPupChange))
            {
                obj.inventory.GiveItem(SS2Content.Items.LuckyPup);
                string name = obj.name.Replace("Master(Clone)", "");
                SS2Log.Info($"Happy Chilean Independence Day to {name}!");
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Main.ChileanIndependenceWeek;
        }
    }
}