using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
#if DEBUG
    public sealed class Roulette : SS2Item
    {
        private const string token = "SS2_ITEM_JETBOOTS_DESC";
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
             * ItemDef - "Roulette" - Items
             */
            yield break;
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Roulette;
            private float stopwatch;
            private static float checkTimer = 0.25f;
            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    stopwatch += Time.fixedDeltaTime;
                    if (stopwatch > checkTimer)
                    {
                        //float currentRouletteBuffs = body.GetBuffCount();
                    }
                }
            }
        }
    }
#endif
}
