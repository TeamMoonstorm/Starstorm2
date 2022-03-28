using Moonstorm.Components;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class ChirrRoot : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrRoot");

        public sealed class Behaviour : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffChirrRoot;
            public void RecalculateStatsStart()
            {

            }

            public void RecalculateStatsEnd()
            {
                //body.moveSpeed = 0f;
                //body.acceleration = 80f;
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedRootCount += 1;
            }
        }
    }
}