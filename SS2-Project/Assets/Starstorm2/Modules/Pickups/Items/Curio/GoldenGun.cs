using RoR2;
using UnityEngine;
using RoR2.ContentManagement;
using R2API;
namespace SS2.Items
{
    public sealed class GoldenGun : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("GoldenGun", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => true;
        public override void Initialize()
        {
            On.RoR2.CharacterMaster.GiveMoney += GiveMoney;
            RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
        }

        // 10% increased money per minute
        private void GiveMoney(On.RoR2.CharacterMaster.orig_GiveMoney orig, CharacterMaster self, uint amount)
        {
            int gun = self.inventory ? self.inventory.GetItemCount(SS2Content.Items.GoldenGun) : 0;
            if (gun > 0)
            {
                int minutes = Mathf.FloorToInt(Run.FixedTimeStamp.tNow / 60f);
                float multiplier = Mathf.Pow(1 + (0.1f * gun), minutes);
                orig(self, (uint)(amount * multiplier));
                return;
            }
            orig(self, amount);
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender.inventory) return;
            if (sender.master && sender.inventory.GetItemCount(SS2Content.Items.GoldenGun) > 0)
            {
                int maxStacks = 20; /////////////////////////////////////////////////////////////////////////////////////////////
                float smallChest = Mathf.Min(sender.master.money / Run.instance.GetDifficultyScaledCost(25), maxStacks);
                args.damageMultAdd += .05f * smallChest;
            }

        }
    }
}
