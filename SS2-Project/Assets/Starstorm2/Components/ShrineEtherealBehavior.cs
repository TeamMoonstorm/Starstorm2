using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2
{
    public class ShrineEtherealBehavior : NetworkBehaviour
    {
        public int maxPurchaseCount = 2;
        public int purchaseCount = 0;
        private float refreshTimer;
        private bool waitingForRefresh;

        [SerializeField]
        public PurchaseInteraction purchaseInteraction;

        public void Start()
        {
            Debug.Log("starting ethereal shrine behavior");
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(ActivateEtherealTerminal);

            purchaseCount = 0;

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

        public void ActivateEtherealTerminal(Interactor interactor)
        {
            //Add shrine use effect EffectManager.SpawnEffect() https://github.com/Flanowski/Moonstorm/blob/0.4/Starstorm%202/Cores/EtherealCore.cs

            if (purchaseCount == 0)
            {
                Debug.Log("Processed first ethereal terminal use...");
                purchaseInteraction.SetAvailable(false);
                purchaseInteraction.contextToken = "SS2_ETHEREAL_WARNING2";
                purchaseInteraction.displayNameToken = "SS2_ETHEREAL_NAME2";
                purchaseCount++;
                refreshTimer = 2;

                CharacterBody body = interactor.GetComponent<CharacterBody>();
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = body,
                    baseToken = "SS2_SHRINE_ETHEREAL_WARN_MESSAGE",
                });

                waitingForRefresh = true;
            }

            else
            {
                Debug.Log("Beginning to activate ethereal terminal.");
                /*if (!NetworkServer.active)
                    return;*/

                purchaseInteraction.SetAvailable(false);
                waitingForRefresh = true;

                if (TeleporterInteraction.instance != null)
                {
                    GameObject teleporterInstance = GameObject.Find("Teleporter1(Clone)");
                    if (teleporterInstance == null)
                    {
                        Debug.Log("Failed to find regular teleporter, searching for Lunar..");
                        teleporterInstance = GameObject.Find("LunarTeleporter Variant(Clone)");
                        if (teleporterInstance == null)
                        {
                            Debug.Log("Could not find teleporter!");
                            return;
                        }

                    }

                    TeleporterUpgradeController tuc = teleporterInstance.GetComponent<TeleporterUpgradeController>();
                    if (tuc != null)
                        tuc.CmdUpdateIsEthereal(true);
                    else
                        return;

                    Ethereal.teleIsEthereal = true;

                    Debug.Log("Set ethereal to true");
                }

                CharacterBody body = interactor.GetComponent<CharacterBody>();
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = body,
                    baseToken = "SS2_SHRINE_ETHEREAL_USE_MESSAGE",
                });
                
                purchaseCount++;
                refreshTimer = 2;
                Debug.Log("Finished ethereal setup from shrine.");
            }
        }
    }
}
