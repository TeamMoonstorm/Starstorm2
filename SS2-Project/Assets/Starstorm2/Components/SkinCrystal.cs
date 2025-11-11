using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using UnityEngine.AddressableAssets;

namespace SS2
{
    public class SkinCrystal : NetworkBehaviour
    {
        public string bodyString;
        public BodyIndex bodyIndex;
        public int skinUnlockID;
        public GameObject model;

        [SerializeField]
        public PurchaseInteraction purchaseInteraction;

        public void Start()
        {
            purchaseInteraction = GetComponent<PurchaseInteraction>();

            if (purchaseInteraction == null)
            {
                SS2Log.Error("SkinCrystal.Start : Skin Crystal failed to find body index or purchase interaction.");
                return;
            }

            bodyIndex = BodyCatalog.FindBodyIndex(bodyString);

            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(UpdateState);
        }

        public void UpdateState(Interactor interactor)
        {
            EntityStateMachine esm = GetComponent<EntityStateMachine>();
            EntityStates.CrystalPickup.DestroyCrystal nextState = new EntityStates.CrystalPickup.DestroyCrystal();
            esm.SetNextState(nextState);
        }
    }
}
