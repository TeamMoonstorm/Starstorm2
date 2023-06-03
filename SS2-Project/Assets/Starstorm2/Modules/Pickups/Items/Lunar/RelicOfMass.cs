using R2API;
using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class RelicOfMass : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfMass", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of health increase. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_RELICOFMASS_DESC", StatTypes.MultiplyByN, 0, "100")]
        public static float healthIncrease = 1f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of which acceleration is divided by.")]
        [TokenModifier("SS2_ITEM_RELICOFMASS_DESC", StatTypes.Default, 1)]
        public static float acclMult = 8f;

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier, IStatItemBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfMass;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseHealthAdd += (body.baseMaxHealth + (body.levelMaxHealth * (body.level - 1))) * (stack * healthIncrease);
                //args.moveSpeedMultAdd += stack / 2;
            }
            public void RecalculateStatsStart()
            {

            }

            public void RecalculateStatsEnd()
            {
                body.acceleration = body.baseAcceleration / (stack * acclMult);
            }
        }
    }
}
