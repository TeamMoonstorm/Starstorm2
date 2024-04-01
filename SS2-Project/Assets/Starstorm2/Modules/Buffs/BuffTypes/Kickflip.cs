using RoR2;
namespace SS2.Buffs
{
    public sealed class Kickflip : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffKickflip", SS2Bundle.Items);
    }
}
