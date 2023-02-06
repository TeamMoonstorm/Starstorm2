using R2API;
using RoR2;
using RoR2.Items;
using System;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class TerminationHelper : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("TerminationHelper", SS2Bundle.Items);

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.TerminationHelper;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.healthMultAdd += RelicOfTermination2.healthMult;
                args.damageMultAdd += RelicOfTermination2.damageMult;
                args.moveSpeedMultAdd += RelicOfTermination2.speedMult;
                args.attackSpeedMultAdd += RelicOfTermination2.atkSpeedMult;
            }

        }
    }
}