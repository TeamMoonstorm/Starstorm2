using MSU;
using RoR2;
using RoR2.ContentManagement;
namespace SS2.Items
{
    // TODO: ShardPoison.asset is in AssetStorm/Base/Elites/SuperElites/AffixSuperPoison/.
    // TODO: Are we even using this? What did Orbeez mean by super poison?
    public sealed class ShardPoison : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("ShardPoison", SS2Bundle.Items);

        public override void Initialize()
        {

        }
        public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;
    }
}
