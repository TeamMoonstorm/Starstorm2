using R2API;
using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class TerminationHelper : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("TerminationHelper");

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.NemBossHelper;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.healthMultAdd += RelicOfTermination.Behavior.healthMult;
                //i assume max hp should be edited too, no? hopefully it comes filled //z
                args.damageMultAdd += RelicOfTermination.Behavior.damageMult;
                args.moveSpeedMultAdd += RelicOfTermination.Behavior.speedMult;
            }
        }
    }
}
