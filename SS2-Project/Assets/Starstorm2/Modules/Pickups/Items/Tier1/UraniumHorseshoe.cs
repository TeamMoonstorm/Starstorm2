using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Items
{
#if DEBUG
    public sealed class UraniumHoreshoe : SS2Item
    {
        private const string token = "SS2_ITEM_URANIUMHORSESHOE_DESC";
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "UraniumHoreshoe" - Items
             */
            yield break;
        }
    }
#endif
}