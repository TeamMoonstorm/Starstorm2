using RoR2;
using RoR2.Hologram;
using UnityEngine; 
using UnityEngine.Networking; 
namespace SS2 
{ 
    public class ShrineEtherealBehavior : NetworkBehaviour, IHologramContentProvider
    {
        private MeshRenderer difficultyDisplay;
 
        [SerializeField] 
        public ChildLocator childLocator; 
        [SerializeField] 
        public PurchaseInteraction purchaseInteraction; 
        [SerializeField] 
        public HologramProjector valueProjector;
        
        public void Start() 
        { 
            purchaseInteraction = GetComponent<PurchaseInteraction>(); 
            purchaseInteraction.onDetailedPurchaseServer.AddListener(ActivateEtherealTerminal); 
            TeleporterInteraction.onTeleporterBeginChargingGlobal += DisableShrine; 
            childLocator = GetComponent<ChildLocator>(); 
            
            if (valueProjector)
            {
                valueProjector.contentProvider = this;
            }
        }

        private void DisableShrine(TeleporterInteraction _)
        {
            if (!childLocator) return;
                
            childLocator.FindChild("Burst").gameObject.SetActive(true); 
            gameObject.GetComponent<ShakeEmitter>()?.StartShake(); 

            childLocator.FindChild("Particles").gameObject.SetActive(false); 
            childLocator.FindChild("Symbol").gameObject.SetActive(false); 
            purchaseInteraction.SetAvailable(false);
        }

        private void ActivateEtherealTerminal(CostTypeDef.PayCostContext payCostContext, CostTypeDef.PayCostResults payCostResults) 
        { 
            purchaseInteraction.SetAvailable(false);
            
            if (childLocator) 
            { 
                childLocator.FindChild("Symbol").gameObject.SetActive(false); 
            } 
            
            DisableShrine(null);

            if (TeleporterUpgradeController.instance) 
                TeleporterUpgradeController.instance.UpgradeEthereal(); 

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage() 
            { 
                baseToken = "SS2_SHRINE_ETHEREAL_USE_MESSAGE",
            }); 
                
            Util.PlaySound("EtherealBell", gameObject); 
        } 
        
        // difficulty hologram stuff ,.,.
        public bool ShouldDisplayHologram(GameObject viewer)
        {
            return (!EtherealBehavior.instance.runIsEthereal && purchaseInteraction.available && !EtherealBehavior.instance.runIsEclipse);
        }

        public GameObject GetHologramContentPrefab()
        {
            return SS2Assets.LoadAsset<GameObject>("SymbolDifficulty", SS2Bundle.Indev);
        }

        public void UpdateHologramContent(GameObject hologramContentObject, Transform viewerBody)
        {
            if (difficultyDisplay != null || EtherealBehavior.instance.runIsEthereal) return; // maybe an upgrade arrow or something if its already etherea l>?.,,. idk .,,. something to signify that youre able to hit it again and go even more ethereal ,.,.
            
            difficultyDisplay = hologramContentObject.GetComponent<MeshRenderer>();
            if (!difficultyDisplay) return;
            
            DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(EtherealBehavior.instance.GetUpdatedDifficulty());
            difficultyDisplay.material.mainTexture = difficultyDef.GetIconSprite().texture;
            difficultyDisplay.material.SetColor(TintColor, difficultyDef.color);
                
            ChildLocator difficultyChildLocator = hologramContentObject.GetComponent<ChildLocator>();
            if (!difficultyChildLocator) return;
            
            ParticleSystem fire = difficultyChildLocator.FindChild("Fire").gameObject.GetComponent<ParticleSystem>();
            fire.GetComponent<Renderer>().material.SetColor(TintColor, difficultyDef.color);
            fire.colorOverLifetime.color.gradient.colorKeys[1].color = difficultyDef.color;
                
            ParticleSystem rings = difficultyChildLocator.FindChild("Rings").gameObject.GetComponent<ParticleSystem>();
            rings.GetComponent<Renderer>().material.SetColor(TintColor, difficultyDef.color);
        }
        
        private static readonly int TintColor = Shader.PropertyToID("_TintColor"); // rider is telling me to do this <//3 ,.., 
    } 
} 
