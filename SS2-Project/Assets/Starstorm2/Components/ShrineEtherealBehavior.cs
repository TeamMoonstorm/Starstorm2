using System;
using System.Collections.Generic;
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
        private bool chargeDown; 
        private MeshRenderer difficultyDisplay;
        private ParticleSystem[] chargeParticles;
 
        [SerializeField] 
        public ChildLocator childLocator; 
        [SerializeField] 
        public PurchaseInteraction purchaseInteraction; 
        [SerializeField] 
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

        //reverse the particle system on the rings ,.,. looks nice i think ,.,.,.
        public void Update()
        {
            if (!chargeDown) return;
            
            chargeTimer -= Time.deltaTime * 4f;
            if (chargeTimer <= 0)
            {
                if (childLocator != null)
                {
                    chargeDown = false;
                    chargeParticles = null;
                    childLocator.FindChild("ChargeVFX").gameObject.SetActive(false);
                }
            }
            
            if (chargeParticles == null) return;
            foreach (ParticleSystem chargeSystem in chargeParticles)
            {
                chargeSystem.Simulate(chargeTimer);
            }
        }

        private void DisableShrine(TeleporterInteraction _) 
        { 
            if (childLocator != null) 
            {
                GameObject ChargeVFX = childLocator.FindChild("ChargeVFX").gameObject; 

                if (purchaseCount == 1)
                {
                    ChargeVFX.SetActive(false);
                }
                else
                {
                    //roll that back .,,.,
                    ChildLocator chargeVFXChildLocator = ChargeVFX.GetComponent<ChildLocator>();
                    if (chargeVFXChildLocator)
                    {
                        chargeParticles = new []
                        {
                            chargeVFXChildLocator.FindChild("ChargeRing").GetComponent<ParticleSystem>(),
                            chargeVFXChildLocator.FindChild("Distortion").GetComponent<ParticleSystem>(),
                            chargeVFXChildLocator.FindChild("DistortionRim").GetComponent<ParticleSystem>(),
                        };
                        
                        chargeVFXChildLocator.FindChild("Sparks").GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmitting);
                        chargeVFXChildLocator.FindChild("Lightning").GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmitting);
                        chargeVFXChildLocator.FindChild("PointLight").gameObject.SetActive(false); 

                        chargeDown = true;
                    }
                }
                
                childLocator.FindChild("Burst").gameObject.SetActive(true); 
                ChargeVFX.GetComponent<ShakeEmitter>()?.StartShake(); 

                childLocator.FindChild("Loop").gameObject.SetActive(false); 
                childLocator.FindChild("Particles").gameObject.SetActive(false); 
                childLocator.FindChild("Symbol").gameObject.SetActive(false); 
                purchaseInteraction.SetAvailable(false); 
            } 
            
            waitingForRefresh = false; 
            chargeUp = false; 
            purchaseInteraction.SetAvailable(false); 
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
            
            difficultyDisplay = hologramContentObject.GetComponent<MeshRenderer>();
            if (!difficultyDisplay) return;
            
            DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(EtherealBehavior.instance.GetUpdatedDifficulty());
            difficultyDisplay.material.mainTexture = difficultyDef.GetIconSprite().texture;
            difficultyDisplay.material.SetColor(TintColor, difficultyDef.color);
                
            //make this less messy later <//3 ,.,.
            ChildLocator difficultyChildLocator = hologramContentObject.GetComponent<ChildLocator>();
            if (!difficultyChildLocator) return;
            
            ParticleSystem fire = difficultyChildLocator.FindChild("Fire").gameObject.GetComponent<ParticleSystem>();
            Renderer fireRenderer = fire.GetComponent<Renderer>();
            fireRenderer.material.SetColor(TintColor, difficultyDef.color);
            fire.colorOverLifetime.color.gradient.colorKeys[1].color = difficultyDef.color;
                
            ParticleSystem rings = difficultyChildLocator.FindChild("Rings").gameObject.GetComponent<ParticleSystem>();
            Renderer ringsRenderer = rings.GetComponent<Renderer>();
            ringsRenderer.material.SetColor(TintColor, difficultyDef.color);
        }
        
        private static readonly int TintColor = Shader.PropertyToID("_TintColor"); // rider is telling me to do this <//3 ,.., 
    } 
} 
