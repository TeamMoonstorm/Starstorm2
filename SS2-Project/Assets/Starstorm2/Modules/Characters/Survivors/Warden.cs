using RoR2.ContentManagement;

namespace SS2.Survivors
{
    public class Warden : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acWarden", SS2Bundle.Indev);

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
