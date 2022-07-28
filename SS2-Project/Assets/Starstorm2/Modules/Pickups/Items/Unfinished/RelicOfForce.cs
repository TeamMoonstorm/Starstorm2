using R2API;
using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class RelicOfForce : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfForce");

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfForce;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.damageMultAdd += stack;
                args.attackSpeedMultAdd -= MSUtil.InverseHyperbolicScaling(0.4f, 0.4f, 0.9f, stack);
                args.cooldownMultAdd += MSUtil.InverseHyperbolicScaling(0.4f, 0.4f, 0.9f, stack);
            }
        }
    }
}
