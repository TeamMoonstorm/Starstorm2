using RoR2;

using MSU;
namespace SS2.Items
{
   [DisabledContent]
    public sealed class WonderHerbs : SS2Item
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("WonderHerbs", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.ID_ITEM, ConfigDesc = "Bonus healing per herbs. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_FORK_DESC", StatTypes.MultiplyByN, 0, "100")]
        public static float healBonus = 0.8f;
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
