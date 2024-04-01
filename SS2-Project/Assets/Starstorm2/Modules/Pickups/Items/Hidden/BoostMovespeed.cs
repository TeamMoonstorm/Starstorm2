using R2API;
using RoR2;
namespace SS2.Items
{

    //boosts movespeed by 1% per stack
    public sealed class BoostMovespeed : SS2Item
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("BoostMovespeed", SS2Bundle.Items);

        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddMovespeed;
        }

        private void AddMovespeed(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            float itemCount = sender.inventory ? sender.inventory.GetItemCount(ItemDef) : 0;
            if (itemCount > 0)
                args.moveSpeedMultAdd += itemCount / 100f;
        }
    }
}
