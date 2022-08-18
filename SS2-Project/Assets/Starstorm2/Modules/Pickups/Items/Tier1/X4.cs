using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    
    public sealed class X4 : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("X4");

        [ConfigurableField(ConfigDesc = "Bonus cooldown reduction per X-4 Stimulant.")]
        [TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Default, 0)]
        public static float secCooldown = 0.25f;
        public sealed class Behavior : BaseItemBodyBehavior, IStatItemBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.X4;
            public void RecalculateStatsEnd()
            {
                if (body.skillLocator.secondaryBonusStockSkill)
                    body.skillLocator.secondaryBonusStockSkill.flatCooldownReduction += stack * secCooldown;
                //args.secondaryCooldownMultAdd *= MSUtil.InverseHyperbolicScaling(secCooldown, secCooldown, 0.7f, stack);
            }
            public void RecalculateStatsStart()
            {
            }
        }
    }
}