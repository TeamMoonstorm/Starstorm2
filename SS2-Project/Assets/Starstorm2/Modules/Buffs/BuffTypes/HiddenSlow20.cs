using Moonstorm.Components;
using R2API;
using RoR2;
using System;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class HiddenSlow20 : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdHiddenSlow20", SS2Bundle.Indev);

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdHiddenSlow20;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                int buffCount = body.GetBuffCount(SS2Content.Buffs.bdHiddenSlow20);
                if (buffCount > 0)
                {
                    args.moveSpeedReductionMultAdd += 0.2f * buffCount;
                }
            }
        }
    }
}
