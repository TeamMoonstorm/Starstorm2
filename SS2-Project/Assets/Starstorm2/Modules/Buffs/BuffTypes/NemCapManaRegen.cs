using RoR2;
namespace SS2.Buffs
{
    public sealed class NemCapManaRegen : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdNemCapManaRegen", SS2Bundle.Indev);
    }
}
