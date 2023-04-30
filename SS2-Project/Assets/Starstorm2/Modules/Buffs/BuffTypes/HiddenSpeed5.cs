using Moonstorm.Components;
using R2API;
using RoR2;
using System;

namespace Moonstorm.Starstorm2.Buffs
{
    //>named hiddenspeed"5"
    //>actually a 40% buff (much bigger than 5!)
    //:troll:
    public sealed class HiddenSpeed5 : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdHiddenSpeed5", SS2Bundle.Indev);

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdHiddenSpeed5;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                int buffCount = body.GetBuffCount(SS2Content.Buffs.bdHiddenSpeed5);
                if (buffCount > 0)
                {
                    args.moveSpeedMultAdd += 0.4f * buffCount;
                }
            }
        }
    }
}
