using RoR2;

using Moonstorm;
namespace SS2.Buffs
{
    [DisabledContent]
    public sealed class NucleatorSpecial : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffNucleatorSpecial", SS2Bundle.Indev);
    }
}
