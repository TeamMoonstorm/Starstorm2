using Moonstorm.Starstorm2.Components;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class Remuneration : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Remuneration", SS2Bundle.Items);

        //[RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Chance to gain soul initially. (1 = 100%)")]
        //[TokenModifier("SS2_ITEM_REMUNERATION_DESC", StatTypes.MultiplyByN, 0, "100")]
        //public static float initChance = 0.005f;

        //[RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Soul gain chance cap. (1 = 100%)")]
        //public static float maxChance = 0.1f;

        public override void Initialize()
        {
            base.Initialize();
        }

        public sealed class RemunerationBehavior : BaseItemMasterBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Remuneration;

            private void Awake()
            {
                Stage.onServerStageBegin += TrySpawnShop;
            }

            private void TrySpawnShop(Stage stage)
            {

                Chat.AddMessage("Remuneration moment");

            }
        }
    }
}
