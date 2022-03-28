using RoR2;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class WonderHerbs : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("WonderHerbs");

        [ConfigurableField(ConfigDesc = "Bonus healing per herbs. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_FORK_DESC", StatTypes.Percentage, 0)]
        public static float healBonus = 0.12f;
        public override void Initialize()
        {
            HealthComponent.onCharacterHealServer += BonusHeals;
        }

        private void BonusHeals(HealthComponent healthComponent, float healAmount, ProcChainMask procChainMask)
        {
            int count = healthComponent.body.inventory.GetItemCount(ItemDef);

            healAmount *= 1f + MSUtil.InverseHyperbolicScaling(healBonus, healBonus, 0.6f, count);
        }
    }
}
