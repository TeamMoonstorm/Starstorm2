using RoR2;
using UnityEngine;
using System;
using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;

namespace SS2.Items
{
    public sealed class SantaHat : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acSantaHat", SS2Bundle.Items);

        public static float percentHatChance = 10f;

        public override void Initialize()
        {
            SS2Log.Info("Merry Chirristmas to about " + percentHatChance + "% of you!");
            CharacterMaster.onCharacterMasterDiscovered += GiveHat;
            On.RoR2.CharacterModel.Start += CharacterModel_Start;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Main.ChristmasTime;
        }

        // always enable outside of runs
        // what could possibly go wrong?
        private static void CharacterModel_Start(On.RoR2.CharacterModel.orig_Start orig, CharacterModel self)
        {
            orig(self);
            if (!Run.instance)
            {
                self.EnableItemDisplay(SS2Content.Items.SantaHat.itemIndex);
            }
        }

        private static void GiveHat(CharacterMaster master)
        {
            if (!UnityEngine.Networking.NetworkServer.active) return;

            if (Util.CheckRoll(percentHatChance) && master.inventory)
            {
                master.inventory.GiveItem(SS2Content.Items.SantaHat.itemIndex);
                string name = master.name.Replace("Master(Clone)", "");
                SS2Log.Info("Merry Chirristmas to " + name + "!");
            }
        }
    }
}
