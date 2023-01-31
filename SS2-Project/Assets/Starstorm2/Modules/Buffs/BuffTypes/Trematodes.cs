using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class Trematodes : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffTrematodes", SS2Bundle.Items);
        public static DotController.DotIndex index;

        public override void Initialize()
        {
            index = DotAPI.RegisterDotDef(1, 0.5f, DamageColorIndex.Item, BuffDef);
        }

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffTrematodes;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedReductionMultAdd += DetritiveTrematode.trematodeSlow;
            }
        }
    }
}
