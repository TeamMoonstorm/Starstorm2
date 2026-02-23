using RoR2;
using RoR2.Hologram;
using UnityEngine; 
using UnityEngine.Networking; 
namespace SS2 
{ 
    public class ShrineEtherealBehavior : NetworkBehaviour, IHologramContentProvider
    { 
        public int maxPurchaseCount = 2; 
        public int purchaseCount = 0; 
        private float refreshTimer; 
        private float chargeTimer; 
        private bool waitingForRefresh; 
        private bool chargeUp; 
        private MeshRenderer difficultyDisplay;
 
        [SerializeField] 
        public ChildLocator childLocator; 
        [SerializeField] 
        public PurchaseInteraction purchaseInteraction; 
        
        public HologramProjector valueProjector;
        

        public void Start() 
        { 
            purchaseInteraction = GetComponent<PurchaseInteraction>(); 
            purchaseInteraction.onPurchase.AddListener(ActivateEtherealTerminal); 
            TeleporterInteraction.onTeleporterBeginChargingGlobal += DisableShrine; 
            childLocator = GetComponent<ChildLocator>(); 
 
            purchaseCount = 0; 
        } 
 
        public void FixedUpdate() 
        { 
            if (valueProjector)
            {
                valueProjector.contentProvider = this;
            }
            
            if (waitingForRefresh) 
            { 
                refreshTimer -= Time.fixedDeltaTime; 
                if (refreshTimer <= 0 && purchaseCount < maxPurchaseCount) 
                { 
                    purchaseInteraction.SetAvailable(true); 
                    waitingForRefresh = false; 
                } 
            } 
 
            if (chargeUp) 
            { 
                chargeTimer += Time.fixedDeltaTime; 
                if (chargeTimer >= 6f) 
                { 
                    DisableShrine(null);

                    if (TeleporterUpgradeController.instance) 
                        TeleporterUpgradeController.instance.UpgradeEthereal(); 
 
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage() 
                    { 
                        baseToken = "SS2_SHRINE_ETHEREAL_USE_MESSAGE",
                    }); 
                    
                    Util.PlaySound("EtherealBell", gameObject); 
                } 
            } 
        } 
 
        private void DisableShrine(TeleporterInteraction _) 
        { 
            if (childLocator != null) 
            {
                GameObject ChargeVFX = childLocator.FindChild("ChargeVFX").gameObject; 

                //activate burst effects when fully played out (purchase count 1,.,., otherwise player gets nothing !! 
                if (purchaseCount == 1)
                {
                    SS2Log.Debug("Disabling shrine 1");
                    childLocator.FindChild("Burst").gameObject.SetActive(true); 
                    ChargeVFX.GetComponent<ShakeEmitter>()?.StartShake(); 
                }

                childLocator.FindChild("Loop").gameObject.SetActive(false); 
                childLocator.FindChild("Particles").gameObject.SetActive(false); 
                childLocator.FindChild("Symbol").gameObject.SetActive(false); 
                purchaseInteraction.SetAvailable(false); 
                
                ChargeVFX.SetActive(false); 
                SS2Log.Debug("Disabling shrine 2");
            } 
            
            waitingForRefresh = false; 
            chargeUp = false; 
            purchaseInteraction.SetAvailable(false); 
            SS2Log.Debug("Disabling shrine 3");
        } 
 
        public void ActivateEtherealTerminal(Interactor interactor) 
        { 
            //Add shrine use effect EffectManager.SpawnEffect() https://github.com/Flanowski/Moonstorm/blob/0.4/Starstorm%202/Cores/EtherealCore.cs 
 
            if (purchaseCount == 0) 
            { 
                purchaseInteraction.contextToken = "SS2_SHRINE_ETHEREAL_CONTEXT_CANCEL"; 
                purchaseCount++; 
                refreshTimer = 2; 
                waitingForRefresh = true; 
                purchaseInteraction.SetAvailable(false);
                chargeUp = true; 
                
                Util.PlaySound("Play_UI_shrineActivate", gameObject); 
 
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
                    childLocator.FindChild("ChargeVFX").gameObject.SetActive(true); 
                } 
            } 
            else if (purchaseCount == 1) 
            { 
                Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage 
                { 
                    baseToken = "SS2_SHRINE_ETHEREAL_FAIL_MESSAGE" 
                }); 
                purchaseCount++; 
                
                DisableShrine(null);
            } 
        } 
        
        // difficulty hologram stuff ,.,.
        public bool ShouldDisplayHologram(GameObject viewer)
        {
            return (purchaseCount < 1 && !EtherealBehavior.instance.runIsEthereal);
        }

        public GameObject GetHologramContentPrefab()
        {
            return SS2Assets.LoadAsset<GameObject>("SymbolDifficulty", SS2Bundle.Indev);
        }

        public void UpdateHologramContent(GameObject hologramContentObject, Transform viewerBody)
        {
            if (difficultyDisplay != null || EtherealBehavior.instance.runIsEthereal) return; // maybe an upgrade arrow or something if its already etherea l>?.,,. idk .,,. 
            SS2Log.Debug("updating hologram content prefab for the first time !!");
            difficultyDisplay = hologramContentObject.GetComponent<MeshRenderer>();
            if (difficultyDisplay)
            {
                DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(EtherealBehavior.instance.GetUpdatedDifficulty());
                difficultyDisplay.material.mainTexture = difficultyDef.GetIconSprite().texture;
                difficultyDisplay.material.SetColor("_TintColor", difficultyDef.color);
                
                //make this less messy later <//3 ,.,.
                ChildLocator difficultyChildLocator = hologramContentObject.GetComponent<ChildLocator>();
                ParticleSystem fire = difficultyChildLocator.FindChild("Fire").gameObject.GetComponent<ParticleSystem>();
                Renderer fireRenderer = fire.GetComponent<Renderer>();
                fireRenderer.material.SetColor("_TintColor", difficultyDef.color);
                fire.colorOverLifetime.color.gradient.colorKeys[1].color = difficultyDef.color;
                ParticleSystem.MinMaxGradient startColorFire = fire.main.startColor;
                startColorFire.color = difficultyDef.color;
                
                ParticleSystem rings = difficultyChildLocator.FindChild("Rings").gameObject.GetComponent<ParticleSystem>();
                Renderer ringsRenderer = rings.GetComponent<Renderer>();
                ringsRenderer.material.SetColor("_TintColor", difficultyDef.color);
                ParticleSystem.MinMaxGradient startColorRings = rings.main.startColor;
                startColorRings.color = difficultyDef.color;
            }
        }
    } 
} 
