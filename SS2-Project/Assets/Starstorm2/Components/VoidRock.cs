  using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using UnityEngine.AddressableAssets;
namespace SS2
{
    public class VoidRock : NetworkBehaviour
    {
        [SerializeField]
        public PurchaseInteraction purchaseInteraction;

        public void Start()
        {
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(DropItem);
        }

        public void DropItem(Interactor interactor)
        {
            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(SS2Content.Items.VoidRock.itemIndex), new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Vector3.up * 5f + transform.forward * 2f);
            Util.PlaySound("Play_item_void_treasureCache_open", gameObject);
        }
    }
}
