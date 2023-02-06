using Moonstorm.Components;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class ExecutionerArmor : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffExecutionerArmor", SS2Bundle.Executioner);

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffExecutionerArmor;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //the stacking amounts are added by the item - these base values are here in case the buff is granted by something other than sigil
                args.armorAdd += 60;
            }
        }
    }
}
