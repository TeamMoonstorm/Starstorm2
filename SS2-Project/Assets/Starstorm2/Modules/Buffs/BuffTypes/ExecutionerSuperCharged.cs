using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class ExecutionerSuperCharged : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffExecutionerSuperCharged", SS2Bundle.Executioner);
    }
}
