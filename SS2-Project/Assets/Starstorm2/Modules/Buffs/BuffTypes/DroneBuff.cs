using R2API;
using RoR2;

using MSU;
namespace SS2.Buffs
{
    public sealed class DroneBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdNemCapDroneBuff", SS2Bundle.Indev);

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdOverstress;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.armorAdd += 30f;
                args.baseAttackSpeedAdd += 0.2f;
            }
        }
    }
}
