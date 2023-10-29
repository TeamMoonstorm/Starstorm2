using R2API;
using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class VoidRock : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("VoidRock", SS2Bundle.Indev);

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.VoidRock;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {

            } 
        }
    }
}
