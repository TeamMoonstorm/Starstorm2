using R2API;
using RoR2;
namespace SS2.Buffs
{
    //[DisabledContent]
    public sealed class ChirrConvert : BuffBase
    {
        //just the dot/slow portion of Befriend. check ChirrConvertOrb for the actual converting
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrConvert", SS2Bundle.Chirr);
        public static DotController.DotIndex dotIndex;

        public static float damageCoefficient = 0.8f;
        public override void Initialize()
        {
            dotIndex = DotAPI.RegisterDotDef(.33f, damageCoefficient, DamageColorIndex.Poison, BuffDef);
        }

    }
}
