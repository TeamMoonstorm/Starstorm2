using Moonstorm.Components;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    [DisabledContent]
    public sealed class Wealth : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffScavenger", SS2Bundle.Indev);

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffWealth;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.healthMultAdd += 0.5f;
                args.damageMultAdd += 0.5f;
            }
        }
    }
}
