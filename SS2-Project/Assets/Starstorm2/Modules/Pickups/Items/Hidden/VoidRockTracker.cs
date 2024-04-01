using RoR2;
namespace SS2.Items
{
    public sealed class VoidRockTracker : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("VoidRockTracker", SS2Bundle.Interactables);

    }
}
