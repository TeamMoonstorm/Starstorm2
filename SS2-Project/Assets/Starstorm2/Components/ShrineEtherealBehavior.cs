using RoR2;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2
{
    public class ShrineEtherealBehavior : NetworkBehaviour
    {
        public int maxPurchaseCount = 2;
        public int purchaseCount = 0;
        private float refreshTimer;
        private bool waitingForRefresh;

        [SerializeField]
        public ChildLocator childLocator;
        [SerializeField]
        public PurchaseInteraction purchaseInteraction;

        public void Start()
        {
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(ActivateEtherealTerminal);

            childLocator = GetComponent<ChildLocator>();

            purchaseCount = 0;

            if (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(SS2Content.Artifacts.Adversity))
            {
                var currStage = SceneCatalog.currentSceneDef;
                if (currStage.stageOrder == 5)
                {
                    Deactivate();
                    SS2Log.Debug($"ShrineEtherealBehavior.Start(): Deactivating shrine {this.transform}");
                }
            }
        }

        public void FixedUpdate()
        {
            if (waitingForRefresh)
            {
                refreshTimer -= Time.fixedDeltaTime;
                if (refreshTimer <= 0 && purchaseCount < maxPurchaseCount)
                {
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
                purchaseInteraction.SetAvailable(false);
                purchaseInteraction.contextToken = "SS2_ETHEREAL_WARNING2";
                purchaseInteraction.displayNameToken = "SS2_ETHEREAL_NAME2";
                purchaseCount++;
                refreshTimer = 2;

                Util.PlaySound("Play_UI_shrineActivate", this.gameObject);

                CharacterBody body = interactor.GetComponent<CharacterBody>();
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = body,
                    baseToken = "SS2_SHRINE_ETHEREAL_WARN_MESSAGE",
                });

                if (childLocator != null)
                {
                    childLocator.FindChild("Loop").gameObject.SetActive(true);
                    childLocator.FindChild("Symbol").gameObject.SetActive(false);
                }

                waitingForRefresh = true;
            }
            else
            {
                purchaseInteraction.SetAvailable(false);
                waitingForRefresh = true;

                if (TeleporterInteraction.instance)
                {
                    TeleporterUpgradeController tuc = TeleporterInteraction.instance.GetComponent<TeleporterUpgradeController>();
                    if (tuc != null)
                        tuc.UpdateIsEthereal(true);
                    else
                        return;
                }

                CharacterBody body = interactor.GetComponent<CharacterBody>();
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                {
                    subjectAsCharacterBody = body,
                    baseToken = "SS2_SHRINE_ETHEREAL_USE_MESSAGE",
                });

                if (childLocator != null)
                {
                    childLocator.FindChild("Loop").gameObject.SetActive(false);
                    childLocator.FindChild("Particles").gameObject.SetActive(false);
                    childLocator.FindChild("Burst").gameObject.SetActive(true);

                    childLocator.FindChild("Model").GetComponent<PrintController>().paused = false;
                }

                Util.PlaySound("EtherealBell", this.gameObject);               

                purchaseCount++;
                refreshTimer = 2;
            }
        }

        public void Deactivate()
        {
            purchaseInteraction.SetAvailable(false);
            purchaseCount = maxPurchaseCount;
            waitingForRefresh = true;

            if(childLocator != null)
            {
                childLocator.FindChild("Symbol").gameObject.SetActive(false);
                childLocator.FindChild("Burst").gameObject.SetActive(true);
                childLocator.FindChild("Model").GetComponent<PrintController>().paused = false;
            }

            //Util.PlaySound("EtherealBell", this.gameObject);
        }
    }
}
