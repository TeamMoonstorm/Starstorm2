using RoR2.ContentManagement;

namespace SS2.ItemTiers
{
    public class Curio : SS2ItemTier
    {
        public override SS2AssetRequest<ItemTierAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<ItemTierAssetCollection>("acCurio", SS2Bundle.Base);

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
