using EntityStates.Chirr;
using Moonstorm.Components;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class ChirrAllyBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrAlly");

        public sealed class Behaviour : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffChirrAlly;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.attackSpeedMultAdd += (ChirrMain.allyBuffCoefficient * buffStacks);
                args.moveSpeedMultAdd += (ChirrMain.allyBuffCoefficient * buffStacks);
            }
        }
    }
}
