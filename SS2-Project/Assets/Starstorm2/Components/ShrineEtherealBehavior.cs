using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2
{
    public class ShrineEtherealBehavior : NetworkBehaviour
    {
        public int maxPurchaseCount = 1;
        public int purchaseCount;
        private float refreshTimer;
        private bool waitingForRefresh;

        [SerializeField]
        public PurchaseInteraction purchaseInteraction;

        public void Start()
        {
            Debug.Log("starting ethereal shrine behavior");
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(ActivateEtherealTerminal);

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
        public void ActivateEtherealTerminal(Interactor interactor)
        {
            Debug.Log("Beginning to activate ethereal terminal");
            if (!NetworkServer.active)
                return;

            purchaseInteraction.SetAvailable(false);
            waitingForRefresh = true;

            if (TeleporterInteraction.instance != null)
            {
                Ethereal.teleIsEthereal = true;
                Debug.Log("Set ethereal to true");
            }
            else
                Debug.Log("Teleporter null");

            CharacterBody body = interactor.GetComponent<CharacterBody>();
            Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
            {
                subjectAsCharacterBody = body,
                baseToken = "SS2_SHRINE_ETHEREAL_USE_MESSAGE",
            });
            //Add shrine use effect EffectManager.SpawnEffect() https://github.com/Flanowski/Moonstorm/blob/0.4/Starstorm%202/Cores/EtherealCore.cs

            purchaseCount++;
            refreshTimer = 2;
            Debug.Log("Finished ethereal setup from shrine");
        }
    }
}
