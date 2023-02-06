using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    [DisabledContent]
    public sealed class NucleatorSpecial : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffNucleatorSpecial", SS2Bundle.Indev);
    }
}
