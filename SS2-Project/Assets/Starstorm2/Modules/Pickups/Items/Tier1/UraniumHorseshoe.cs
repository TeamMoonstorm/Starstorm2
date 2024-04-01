using RoR2;
namespace SS2.Items
{
    [DisabledContent]
    public sealed class UraniumHorseshoe : SS2Item
    {

        private const string token = "SS2_ITEM_URANIUMHORSESHOE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("UraniumHorseshoe", SS2Bundle.Items);

        public override void Initialize()
        {

        }

    }
}