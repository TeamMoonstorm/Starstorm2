using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class Gouge : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffGouge");
        public static DotController.DotIndex index;

        public override void Initialize()
        {
            index = DotAPI.RegisterDotDef(0.25f, 0.25f, DamageColorIndex.SuperBleed, BuffDef);
        }
    }
}
