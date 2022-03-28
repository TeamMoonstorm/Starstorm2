using R2API;
using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class CoffeeBag : ItemBase
    {
        public const string token = "SS2_ITEM_COFFEEBAG_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("CoffeeBag");

        [ConfigurableField(ConfigDesc = "Attack speed bonus for each coffee bag. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 0)]
        public static float atkSpeedBonus = 0.075f;

        [ConfigurableField(ConfigDesc = "Movement speed bonus for each coffee bag. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 1)]
        public static float moveSpeedBonus = 0.07f;

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier//IStatItemBehavior
        {
            /* I've decided to leave this part here as an example on how to move from the IStatItemBehavior to the IBodyStatArgModifier interface.
            /*public void RecalculateStatsStart()
            { }

            public void RecalculateStatsEnd()
            {
                body.attackSpeed += (body.baseAttackSpeed + body.levelAttackSpeed * (body.level - 1)) * atkSpeedBonus * stack;
                body.moveSpeed += (body.baseMoveSpeed + body.levelMoveSpeed * (body.level - 1)) * moveSpeedBonus * stack;
            }*/
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.CoffeeBag;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseAttackSpeedAdd += (body.baseAttackSpeed + body.levelAttackSpeed * (body.level - 1)) * atkSpeedBonus * stack;
                args.baseMoveSpeedAdd += (body.baseMoveSpeed + body.levelMoveSpeed * (body.level - 1)) * moveSpeedBonus * stack;
            }
        }
    }
}
