using MSU;
using R2API;
using RoR2;
using EntityStates.VoidCamp;
using RoR2.ContentManagement;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class ShardStorm : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("ShardStorm", SS2Bundle.Items);

        public override void Initialize()
        {

        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta && base.IsAvailable(contentPack);
        }
    }
}
