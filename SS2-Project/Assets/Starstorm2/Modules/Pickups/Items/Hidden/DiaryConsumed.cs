using RoR2;
namespace SS2.Items
{
    public sealed class DiaryConsumed : SS2Item
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("DiaryConsumed", SS2Bundle.Items);
    }
}
