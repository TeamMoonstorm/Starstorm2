using RoR2;
namespace SS2.Buffs
{
    public sealed class JetBootsCooldown : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffJetBootsCooldown", SS2Bundle.Items);
    }
}