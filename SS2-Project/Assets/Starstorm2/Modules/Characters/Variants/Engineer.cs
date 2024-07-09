using Assets.Starstorm2.ContentClasses;
using SS2;
using RoR2;
using RoR2.ContentManagement;

namespace Assets.Starstorm2.Modules.Characters.Variants
{
    public class Engineer : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<AssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<AssetCollection>("acEngineer", SS2Bundle.Indev);


        public override void Initialize()
        {
        }


        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
