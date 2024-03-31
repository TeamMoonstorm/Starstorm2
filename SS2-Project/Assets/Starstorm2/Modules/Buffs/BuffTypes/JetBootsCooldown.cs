using RoR2;
using Moonstorm;
namespace SS2.Buffs
{
    public sealed class JetBootsCooldown : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffJetBootsCooldown", SS2Bundle.Items);
    }
}