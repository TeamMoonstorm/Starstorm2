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

        [SerializeField]
        public PurchaseInteraction purchaseInteraction;

        public void Start()
        {
            purchaseInteraction = GetComponent<PurchaseInteraction>();

            if (purchaseInteraction == null)
            {
                SS2Log.Error("Skin Crystal failed to find body index or purchase interaction.");
                return;
            }

            bodyIndex = BodyCatalog.FindBodyIndex(bodyString);
        }
    }
}
