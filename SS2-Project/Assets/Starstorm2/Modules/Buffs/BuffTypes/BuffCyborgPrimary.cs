using RoR2;
namespace SS2.Buffs
{
    //[DisabledContent]
    public sealed class BuffCyborgPrimary : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffCyborgPrimary", SS2Bundle.Indev);
    }
}
