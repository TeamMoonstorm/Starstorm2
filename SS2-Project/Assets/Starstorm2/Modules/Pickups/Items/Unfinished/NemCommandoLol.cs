using RoR2;
namespace SS2.Items
{
    [DisabledContent]
    public sealed class NemCommandoLol : SS2Item
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("NemCommandoLol", SS2Bundle.Nemmando);

    }
}
