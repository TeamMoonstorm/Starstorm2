using R2API;
using RoR2;
namespace SS2.Buffs
{
    public sealed class Intoxicated : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffIntoxicated", SS2Bundle.Items);
        public static BuffDef buff;
        public static DotController.DotIndex index;

        public override void Initialize()
        {
            buff = BuffDef;
            index = DotAPI.RegisterDotDef(1/3f, 1/3f, DamageColorIndex.Poison, BuffDef);
        }
    }
}
