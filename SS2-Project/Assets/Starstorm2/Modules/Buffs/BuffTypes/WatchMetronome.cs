using Moonstorm.Components;
using R2API;
using RoR2;
using System;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class WatchMetronome : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffWatchMetronome", SS2Bundle.Items);

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffWatchMetronome;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                int buffCount = body.GetBuffCount(SS2Content.Buffs.BuffWatchMetronome);
                if (buffCount > 0 && body.isSprinting)
                {
                    args.moveSpeedMultAdd += (float)Math.Sqrt(0.1f * buffCount) * Items.WatchMetronome.maxMovementSpeed;
                }
            }
        }
    }
}
