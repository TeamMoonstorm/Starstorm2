using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class Insecticide : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffInsecticide");
        public static DotController.DotIndex index;

        public override void Initialize()
        {
            index = DotAPI.RegisterDotDef(8, 0.125f, DamageColorIndex.Poison, BuffDef);
        }
    }
}
