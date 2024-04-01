using RoR2;
using RoR2.Items;
using UnityEngine;
namespace SS2.Items
{
    public sealed class FlowerTurret : SS2Item
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("FlowerTurret", SS2Bundle.Items);

        public override GameObject ItemDisplayPrefab => SS2Assets.LoadAsset<GameObject>("DisplayFlowerTurret", SS2Bundle.Items);

        public override void Initialize()
        {

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
