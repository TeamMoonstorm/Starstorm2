using SS2;
using RoR2;
using RoR2.ContentManagement;

namespace SS2.Survivors
{
    public class Artificer : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<VanillaSurvivorAssetCollection>("acArtificer", SS2Bundle.Indev);


        public override void Initialize()
        {
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }
    }
}
