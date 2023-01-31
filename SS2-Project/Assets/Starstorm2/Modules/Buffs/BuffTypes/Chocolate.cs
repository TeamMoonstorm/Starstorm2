using Moonstorm.Components;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class Chocolate : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChocolate", SS2Bundle.Items);

        //TODO: Add choccy effect
        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffChocolate;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.critAdd += Items.GreenChocolate.buffCrit * buffStacks;
                args.damageMultAdd += Items.GreenChocolate.buffDamage * buffStacks;
            }
        }
    }
}
