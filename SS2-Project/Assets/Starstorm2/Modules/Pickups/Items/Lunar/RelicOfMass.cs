using R2API;
using RoR2;
using RoR2.Items;

using MSU;
namespace SS2.Items
{
    public sealed class RelicOfMass : SS2Item
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfMass", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of health increase. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_RELICOFMASS_DESC", StatTypes.MultiplyByN, 0, "100")]
        public static float healthIncrease = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of which acceleration is divided by.")]
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
