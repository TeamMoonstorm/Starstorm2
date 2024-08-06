using RoR2.ContentManagement;

namespace SS2.ItemTiers
{
#if DEBUG
    public class Relic : SS2ItemTier
    {
        public override SS2AssetRequest<ItemTierAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<ItemTierAssetCollection>("acRelic", SS2Bundle.Base);

        public override void Initialize()
        {

        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }
    }
#endif
}