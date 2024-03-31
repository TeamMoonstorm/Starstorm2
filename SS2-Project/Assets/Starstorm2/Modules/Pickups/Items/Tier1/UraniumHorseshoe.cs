using RoR2;

using Moonstorm;
namespace SS2.Items
{
    [DisabledContent]
    public sealed class UraniumHorseshoe : ItemBase
    {

        private const string token = "SS2_ITEM_URANIUMHORSESHOE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("UraniumHorseshoe", SS2Bundle.Items);

        public override void Initialize()
        {

        }

    }
}