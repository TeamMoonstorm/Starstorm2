using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using System;
namespace Moonstorm.Starstorm2.Items
{
    public sealed class SantaHat : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("SantaHat", SS2Bundle.Items);

        public override GameObject ItemDisplayPrefab => SS2Assets.LoadAsset<GameObject>("DisplaySantaHat", SS2Bundle.Items);

        public static float percentHatChance = 10f;

        public override void Initialize()
        {
            DateTime today = DateTime.Today;
            if (today.Month == 12)
            {
                SS2Log.Info("Merry Chirristmas to about " + percentHatChance + "% of you!");
                CharacterMaster.onCharacterMasterDiscovered += GiveHat;
                On.RoR2.CharacterModel.Start += CharacterModel_Start;
            }
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
