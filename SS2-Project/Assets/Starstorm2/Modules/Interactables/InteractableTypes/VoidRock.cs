using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2
{
    public class VoidRock : NetworkBehaviour
    {
        public int maxPurchaseCount = 1;
        public int purchaseCount = 0;
        private float refreshTimer;
        private bool waitingForRefresh;

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
            CharacterBody body = interactor.GetComponent<CharacterBody>();
            Items.VoidRock.invasionStage = true;
            Items.VoidRock.setStage = true;
            Items.VoidRock.initialStage = Run.instance.stageClearCount;
            Items.VoidRock.inventory = body.inventory;
            Util.PlaySound("Play_item_void_treasureCache_open", gameObject);
            if (body != null)
            {
                //CreateItemTakenOrb(gameObject.transform.position, body.gameObject, SS2Content.Items.VoidRock.itemIndex);
                //body.inventory.GiveItem(SS2Content.Items.VoidRock.itemIndex, 1);                //CharacterMasterNotificationQueue.PushItemNotification(body.master, SS2Content.Items.VoidRock.itemIndex);
            }
        }

        [Server]
        public static void CreateItemTakenOrb(Vector3 effectOrigin, GameObject targetObject, ItemIndex itemIndex)
        {
            if (!NetworkServer.active)
            {
                Debug.Log("client client client @ Void Rock CreateItemTakenOrb");
                return;
            }
            
            GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ItemTakenOrbEffect.prefab").WaitForCompletion();
            EffectData effectData = new EffectData
            {
                origin = effectOrigin,
                genericFloat = 1.5f,
                genericUInt = (uint)(itemIndex + 1)
            };
            effectData.SetNetworkedObjectReference(targetObject);
            EffectManager.SpawnEffect(effectPrefab, effectData, true);
        }
    }
}
