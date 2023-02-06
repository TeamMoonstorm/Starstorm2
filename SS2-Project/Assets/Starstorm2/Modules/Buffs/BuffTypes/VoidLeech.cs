using Moonstorm.Components;
using R2API;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    [DisabledContent]
    public sealed class VoidLeech : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffVoidLeech", SS2Bundle.Items);

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier//, IStatItemBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffVoidLeech;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedReductionMultAdd += (1 - Mathf.Pow(0.9f, buffStacks));
                args.jumpPowerMultAdd *= Mathf.Pow(0.9f, buffStacks);
            }
            /*
            public void RecalculateStatsStart()
            {
            }
            public void RecalculateStatsEnd()
            {
                body.jumpPower *= Mathf.Pow(0.9f, buffStacks);
            }*/

        }
    }
}
