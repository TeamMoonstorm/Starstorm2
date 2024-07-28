using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.ItemTiers
{
    public class Relic : SS2ItemTier
    {
        public override SS2AssetRequest<ItemTierAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<ItemTierAssetCollection>("acRelic", SS2Bundle.Base);

        public override void Initialize()
        {

        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}