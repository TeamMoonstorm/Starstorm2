using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SS2.Components
{
    //GOUFA :)
    public class GameObjectUnlockableFilterAddressableHelper : MonoBehaviour
    {
        private GameObjectUnlockableFilter gouf;
        private bool modifiedGouf = false;
        private UnlockableDef requiredUnlockableDef;
        private UnlockableDef forbiddenUnlockableDef;
        public string requiredUnlockableAddressableAddress;
        public string forbiddenUnlockableAddressableAddress;

        public void Start()
        {
            gouf = GetComponent<GameObjectUnlockableFilter>();
            if (gouf == null)
            {
                SS2Log.Error("GameObjectUnlockableFilterAddressableHelper : GOUF null");
                return;
            }

            if (requiredUnlockableAddressableAddress != null)
            {
                requiredUnlockableDef = Addressables.LoadAssetAsync<UnlockableDef>(requiredUnlockableAddressableAddress).WaitForCompletion();
                if (requiredUnlockableDef != null)
                {
                    gouf.requiredUnlockableDef = requiredUnlockableDef;
                    modifiedGouf = true;
                }
                else
                {
                    SS2Log.Error("GOUFAHelper : Could not find addressable unlockabledef from string " + requiredUnlockableAddressableAddress);
                }
            }

            if (forbiddenUnlockableAddressableAddress != null)
            {
                forbiddenUnlockableDef = Addressables.LoadAssetAsync<UnlockableDef>(forbiddenUnlockableAddressableAddress).WaitForCompletion();
                if (forbiddenUnlockableDef != null)
                {
                    gouf.forbiddenUnlockableDef = forbiddenUnlockableDef;
                    modifiedGouf = true;
                }
                else
                {
                    SS2Log.Error("GOUFAHelper : Could not find addressable unlockabledef from string " + forbiddenUnlockableAddressableAddress);
                }
            }

            gouf.enabled = true;
        }
    }
}
