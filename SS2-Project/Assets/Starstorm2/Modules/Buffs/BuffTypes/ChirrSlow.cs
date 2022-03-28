using EntityStates.Chirr;
using Moonstorm.Components;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class ChirrSlow : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrSlow");

        public sealed class Behaviour : BaseBuffBodyBehavior, IStatItemBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffChirrSlow;
            public void RecalculateStatsStart()
            {

            }

            public void RecalculateStatsEnd()
            {
                body.moveSpeed *= (1 - (ChirrMain.enemySlowCoefficient * buffStacks));
                body.acceleration *= (1 - (ChirrMain.enemySlowCoefficient * buffStacks));
            }
        }
    }
}
