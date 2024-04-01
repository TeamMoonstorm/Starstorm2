using RoR2;
namespace SS2.Buffs
{
    public sealed class CoffeeBag : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffCoffeeBag", SS2Bundle.Items);
    }
}
