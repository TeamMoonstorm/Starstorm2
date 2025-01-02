using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Items
{
    //lets a body use multiple elite buffs from equipment slots. includes overlays and itemdisplays
    //behavior is in ExtraEquipmentManager nvm
    public sealed class MultiElite : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("MultiElite", SS2Bundle.Items);
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}