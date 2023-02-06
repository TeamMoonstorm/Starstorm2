using EntityStates.Beastmaster;
using Moonstorm.Components;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class ChirrAllyBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrAlly", SS2Bundle.Indev);

        public sealed class Behaviour : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffChirrAlly;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //fixxxxxxxxx laterrrr
                //args.attackSpeedMultAdd += (BeastmasterMain.allyBuffCoefficient * buffStacks);
                //args.moveSpeedMultAdd += (BeastmasterMain.allyBuffCoefficient * buffStacks);
            }
        }
    }
}
