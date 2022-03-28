using EntityStates.Chirr;
using Moonstorm.Components;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class ChirrFly : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrFly");

        public sealed class Behaviour : BaseBuffBodyBehavior, IBodyStatArgModifier, IStatItemBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffChirrFly;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedMultAdd += (ChirrMain.allyBuffCoefficient * buffStacks);
                //Acceleration not yet supported by RecalcStatsAPI
            }
            public void RecalculateStatsStart()
            {
                // hehe hi
                // hello :3
            }

            public void RecalculateStatsEnd()
            {
                body.acceleration *= (ChirrMain.allyBuffCoefficient * 2) * buffStacks;
            }
        }
    }
}
