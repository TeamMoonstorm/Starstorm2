using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2
{
    public class NemSpawnPrefabBehavior : NetworkBehaviour
    {
        public int maxPurchaseCount = 1;
        public int purchaseCount;
        private float refreshTimer;
        private bool waitingForRefresh;

        [SerializeField]
        public PurchaseInteraction purchaseInteraction;

        public void Start()
        {
            Debug.Log("starting nem spawn prefab behavior");
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(Emerge);

            if (purchaseInteraction == null)
                Debug.Log("pi null");
            else
                Debug.Log("pi set");
        }

        public void FixedUpdate()
        {
            if (waitingForRefresh)
            {
                refreshTimer -= Time.fixedDeltaTime;
                if (refreshTimer <= 0 && purchaseCount < maxPurchaseCount)
                {
                    Debug.Log("set to avaliable");
                    purchaseInteraction.SetAvailable(true);
                    waitingForRefresh = false;
                }
            }
        }

        [Server]
        public void Emerge(Interactor interactor)
        {
            Debug.Log("killing self");
            if (!NetworkServer.active)
                return;

            purchaseInteraction.SetAvailable(false);
            waitingForRefresh = true;

            //Destroy(gameObject);

            purchaseCount++;
            refreshTimer = 2;
            Debug.Log("i should be dead by now!!!");
        }
    }
}
