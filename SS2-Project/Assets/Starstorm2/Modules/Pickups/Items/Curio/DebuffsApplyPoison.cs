using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
namespace SS2.Items
{
    public sealed class DebuffsApplyPoison : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("DebuffsApplyPoison", SS2Bundle.Items);
        public override bool IsAvailable(ContentPack contentPack) => true;
        public override void Initialize()
        {

        }
    }
}
