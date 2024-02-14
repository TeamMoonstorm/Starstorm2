using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class VoidRock : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("VoidRock", SS2Bundle.Interactables);

        public static int initialStage = 0;
        public static bool setStage = false;
        public static bool invasionStage = false;
        public static Inventory inventory;

        public override void Initialize()
        {
            base.Initialize();

            Run.onRunStartGlobal += Run_onRunStartGlobal;
        }

        private static void Run_onRunStartGlobal(Run run)
        {
            initialStage = 0;
            setStage = false;
            invasionStage = false;
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
                /*else
                {
                    setStage = true;
                    initialStage = Run.instance.stageClearCount;
                    inventory = body.inventory;
                    invasionStage = true;
                }*/
            }
        }
    }
}
