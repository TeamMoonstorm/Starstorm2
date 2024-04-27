using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Items
{
    public sealed class FlowerTurret : SS2Item
    {
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return contentPack.survivorDefs.Find("Chirr");
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * GameObject - "DisplayFlowerTurret" - Chirr
             * ItemDef - "FlowerTurret" - Chirr
             */
            yield break;
        }

        public sealed class BodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.FlowerTurret;


            private GameObject flowerTurretController;
            public void Start()
            {
                this.flowerTurretController = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("FlowerTurretController", SS2Bundle.Items));
                this.flowerTurretController.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject);
            }

            private void OnDestroy()
            {
                Destroy(this.flowerTurretController);
            }
        }
    }
}
