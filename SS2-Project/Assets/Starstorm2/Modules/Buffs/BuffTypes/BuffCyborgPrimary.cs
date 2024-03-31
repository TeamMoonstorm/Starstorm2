using RoR2;

using Moonstorm;
namespace SS2.Buffs
{
    //[DisabledContent]
    public sealed class BuffCyborgPrimary : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffCyborgPrimary", SS2Bundle.Indev);
    }
}
