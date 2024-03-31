using RoR2;

using Moonstorm;
namespace SS2.Buffs
{
    public sealed class NeedleBuildup : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffNeedleBuildup", SS2Bundle.Items);
    }
}
