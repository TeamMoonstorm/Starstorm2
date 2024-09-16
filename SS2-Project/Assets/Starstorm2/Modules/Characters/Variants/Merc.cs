using SS2;
using RoR2;
using RoR2.ContentManagement;
using MSU;

namespace SS2.Survivors
{
    public class Merc : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<VanillaSurvivorAssetCollection>("acMerc", SS2Bundle.Vanilla);


        public override void Initialize()
        {
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
