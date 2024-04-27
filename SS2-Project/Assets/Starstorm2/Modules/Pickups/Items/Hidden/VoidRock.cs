using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Items
{
    public sealed class VoidRock : SS2Item
    {
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public static int initialStage = 0;
        public static bool setStage = false;
        public static bool invasionStage = false;
        public static Inventory inventory;

        public override void Initialize()
        {
            Run.onRunStartGlobal += Run_onRunStartGlobal;
        }


        private static void Run_onRunStartGlobal(Run run)
        {
            initialStage = 0;
            setStage = false;
            invasionStage = false;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "VoidRock" - Interactables
             * ItemDef - "VoidRockTracker" - Interactables
             */
            yield break;
        }

        public sealed class VoidRockBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.VoidRock;

            public new void Awake()
            {
                base.Awake();
                Debug.Log("invasionStage : " + invasionStage);
                Debug.Log("initialStage : " + initialStage);
            }

            public void OnDestroy()
            {
                if (Run.instance == null)
                {
                    setStage = false;
                    invasionStage = false;
                    initialStage = 0;
                    return;
                }

                if (setStage)
                {
                    if ((Run.instance.stageClearCount - initialStage) % 3 == 0)
                    {
                        invasionStage = true;
                    }
                    else
                    {
                        invasionStage = false;
                    }
                }
            }
        }
    }
}
