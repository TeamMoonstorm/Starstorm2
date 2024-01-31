using RoR2;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class CompositeInjector : ItemBase
    {

        private const string token = "SS2_ITEM_COMPOSITEINJECTOR_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("CompositeInjector", SS2Bundle.Items);

        public override void Initialize()
        {

        }

    }
}