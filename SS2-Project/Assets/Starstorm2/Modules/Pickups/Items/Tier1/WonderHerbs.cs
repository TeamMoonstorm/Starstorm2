using RoR2;

using MSU;
namespace SS2.Items
{
   [DisabledContent]
    public sealed class WonderHerbs : SS2Item
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("WonderHerbs", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Bonus healing per herbs. (1 = 100%)")]
        [FormatToken("SS2_ITEM_FORK_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 0, "100")]
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
