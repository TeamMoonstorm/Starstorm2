using RoR2;
namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class JetBootsCooldown : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffJetBootsCooldown", SS2Bundle.Items);
    }
}