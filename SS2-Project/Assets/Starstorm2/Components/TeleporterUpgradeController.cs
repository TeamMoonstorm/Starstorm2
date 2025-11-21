using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
namespace SS2
{
    public class TeleporterUpgradeController : NetworkBehaviour
    {
        public static TeleporterUpgradeController instance;
        [SystemInitializer]
        private static void Init()
        {
            //Add teleporter upgrading component to teleporters
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/Teleporter1.prefab").WaitForCompletion().AddComponent<TeleporterUpgradeController>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/LunarTeleporter Variant.prefab").WaitForCompletion().AddComponent<TeleporterUpgradeController>();
        }
        [SyncVar]
        public bool isEthereal = false;
        [SyncVar]
        public bool isStorm = false;

        private Transform teleBase;
        private HoldoutZoneController hzc;
        private TeleporterInteraction ti;
        private ParticleSystem telePassiveParticles;
        private ParticleSystem teleCenterParticles;
        private ParticleSystem lightningParticles;
        private Light lightningLightRef;
        private Renderer rangeIndicator;
        private ParticleSystem betweenProngsCore;
        private Light coreLightRef;
        private ParticleSystem debris;
        private ParticleSystem chargingRing;
        private Light loopLight;
        private ParticleSystem core;
        private ParticleSystem beam;

        //private SpriteRenderer teleporterSprite;
        private GameObject portalIndicator;

        private Transform stormIcon;
        private Transform etherealIcon;
        private static float iconHeightOffset = 2.4f;
        private void Awake()
        {
            hzc = GetComponent<HoldoutZoneController>();
            ti = GetComponent<TeleporterInteraction>();

            teleBase = ti.transform.Find("TeleporterBaseMesh");           
            var teleProngs = teleBase.transform.Find("TeleporterProngMesh").gameObject;
            var teleBeacon = teleBase.transform.Find("SurfaceHeight/TeleporterBeacon").gameObject;

            stormIcon = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("StormTeleporterSymbol", SS2Bundle.Events), teleBase).transform;
            stormIcon.gameObject.SetActive(false);
            etherealIcon = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("EtherealTeleporterSymbol", SS2Bundle.Indev), teleBase).transform;
            etherealIcon.gameObject.SetActive(false);

            //passive vfx
            Transform effects = teleBase.Find("BuiltInEffects");
            telePassiveParticles = effects.Find("PassiveParticle, Sphere").GetComponent<ParticleSystem>();
            teleCenterParticles = effects.Find("PassiveParticle, Center").GetComponent<ParticleSystem>();
            lightningParticles = effects.Find("ChargedEffect/LightningAlongProngs").GetComponent<ParticleSystem>();
            lightningLightRef = effects.Find("ChargedEffect/LightningAlongProngs/ReferencePointLight").GetComponent<Light>();
            betweenProngsCore = effects.Find("IdleToChargingEffect/BetweenProngs/Core").GetComponent<ParticleSystem>();
            coreLightRef = effects.Find("IdleToChargingEffect/BetweenProngs/Point light").GetComponent<Light>();
            debris = effects.Find("IdleToChargingEffect/3DDebris").GetComponent<ParticleSystem>();
            chargingRing = effects.Find("ChargingEffect/Ring").GetComponent<ParticleSystem>();
            loopLight = effects.Find("ChargingEffect/BetweenProngs/Loop/Point light").GetComponent<Light>();
            core = effects.Find("ChargingEffect/BetweenProngs/Loop/Core").GetComponent<ParticleSystem>();
            beam = effects.Find("ChargingEffect/BetweenProngs/Loop/Beam").GetComponent<ParticleSystem>();

            //teleporterSprite = GameObject.Find("TeleporterChargingPositionIndicator(Clone)/TeleporterSprite").GetComponent<SpriteRenderer>();

            rangeIndicator = hzc.radiusIndicator;

            TeleporterInteraction.onTeleporterBeginChargingGlobal += OnTeleporterBeginChargingGlobal;
            TeleporterInteraction.onTeleporterChargedGlobal += OnTeleporterChargedGlobal;

            if(ti.bossDirector)
                ti.bossDirector.onSpawnedServer.AddListener(new UnityEngine.Events.UnityAction<GameObject>(ModifySpawnedBoss));
        }       
        private void OnDestroy()
        {
            TeleporterInteraction.onTeleporterBeginChargingGlobal -= OnTeleporterBeginChargingGlobal;
            TeleporterInteraction.onTeleporterChargedGlobal -= OnTeleporterChargedGlobal;
        }
        private void OnEnable()
        {
            instance = this;
        }
        private void OnDisable()
        {
            instance = null;
        }
        private void OnTeleporterBeginChargingGlobal(TeleporterInteraction tele)
        {
            if (isEthereal)
            {
                if (tele.bossDirector)
                {
                    tele.bossDirector.monsterCredit += (float)(int)(100f * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f));
                }
                if (tele.bonusDirector)
                {
                    tele.bonusDirector.monsterCredit += (float)(int)(50f * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f));
                }
            }

        }
        private void OnTeleporterChargedGlobal(TeleporterInteraction teleporter)
        {
            if (isEthereal)
            {
                if (portalIndicator) Destroy(portalIndicator);
                EtherealBehavior.instance.OnEtherealTeleporterCharged();
                teleporter.AttemptSpawnPortal(SS2Assets.LoadAsset<InteractableSpawnCard>("iscStrangerPortal", SS2Bundle.SharedStages), 20f, 30f, "hehe oortal");
            }               
        }

        private void ModifySpawnedBoss(GameObject masterObject)
        {
            if(isEthereal)
            {
                CharacterMaster bodyMaster = masterObject.GetComponent<CharacterMaster>();
                // TODO: move all this item granting to one class. its getting tricky to keep track of
                bodyMaster.inventory.GiveItem(SS2Content.Items.EtherealItemAffix);
                bodyMaster.inventory.GiveItem(RoR2Content.Items.BoostHp, (int)(14 + (7 * EtherealBehavior.instance.etherealsCompleted)));
                bodyMaster.inventory.GiveItem(SS2Content.Items.MaxHealthPerMinute, 1 + EtherealBehavior.instance.etherealsCompleted);
            }
        }

        private void FixedUpdate()
        {
            stormIcon.gameObject.SetActive(isStorm && !ti.isCharged);
            etherealIcon.gameObject.SetActive(isEthereal && !ti.isCharged);
            Vector3 position = ti._bossShrineCounter.transform.localPosition;
            float z = 0;

            // Buns: Adding a cautious null check here post-AC since I dont understand how this code worked previously and dont 
            // wanna break someones game over an indicator error
            if (position != null)
            {
                // Buns: The new bossShrineCounter added support for multiple indicators and removed activeSelf param
                // Buns: idk if this will work but need to fix the compile error to get SS2 to work so YOLO
                // TODO: Might be the source of a bug!
                if (ti._bossShrineCounter.indicatorCount == 0)
                {
                    z += iconHeightOffset;
                }
                else if (ti._bossShrineCounter.indicatorCount >= 1)
                {
                    z += iconHeightOffset * ti._bossShrineCounter.indicatorCount;
                }

                if (isEthereal)
                {
                    etherealIcon.transform.localPosition = position + Vector3.forward * z;
                    z += iconHeightOffset;
                }
                if (isStorm)
                {
                    stormIcon.transform.localPosition = position + Vector3.forward * z;
                    z += iconHeightOffset;
                }
            }
        }

        [Server]
        public void UpgradeStorm(bool upgrade) // just gonna use this for visuals for now. too lazy to move stuff here. behavior is in EntityStates/Events/Weather/Storm.cs
        {
            if (upgrade == isStorm)
                return;
            isStorm = upgrade;
            if (upgrade) 
                EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("StormTeleporterUpgradeEffect", SS2Bundle.Events), ti.transform.position, Quaternion.identity, true);
            RpcUpgradeStorm(upgrade);
        }
        [ClientRpc]
        public void RpcUpgradeStorm(bool upgrade)
        {
            if(upgrade)
            {
                Material stormOverlay = SS2Assets.LoadAsset<Material>("matStormTeleporterOverlay", SS2Bundle.Events);
                AppendMaterial(teleBase.gameObject.GetComponent<Renderer>(), stormOverlay);
                AppendMaterial(teleBase.Find("TeleporterProngMesh").gameObject.GetComponent<Renderer>(), stormOverlay);
                AppendMaterial(teleBase.Find("SurfaceHeight/TeleporterBeacon").gameObject.GetComponent<Renderer>(), stormOverlay);
                void AppendMaterial(Renderer renderer, Material material)
                {
                    Material[] materials = renderer.sharedMaterials;
                    HG.ArrayUtils.ArrayAppend(ref materials, material);
                    renderer.sharedMaterials = materials;
                }
            }
            else
            {
                RemoveMaterial(teleBase.gameObject.GetComponent<Renderer>(), 2); // XDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
                RemoveMaterial(teleBase.Find("TeleporterProngMesh").gameObject.GetComponent<Renderer>(), 2);
                RemoveMaterial(teleBase.Find("SurfaceHeight/TeleporterBeacon").gameObject.GetComponent<Renderer>(), 2);
                void RemoveMaterial(Renderer renderer, int index)
                {
                    Material[] materials = renderer.sharedMaterials;
                    HG.ArrayUtils.ArrayRemoveAtAndResize(ref materials, index);
                    renderer.sharedMaterials = materials;
                }

            }
        }
        
        [Server]
        public void UpgradeEthereal()
        {
            if (isEthereal) return;
            isEthereal = true;
            EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("EtherealTeleporterUpgradeEffect", SS2Bundle.Indev), ti.transform.position, Quaternion.identity, true);
            RpcUpgradeEthereal();
        }

        [ClientRpc]
        public void RpcUpgradeEthereal()
        {         
            EtherealUpgradeInternal();
        }

        private void EtherealUpgradeInternal() // not gonna bother undoing this. will never happen in gameplay
        {
            // spawn portal orb
            if (ti.modelChildLocator)
            {
                portalIndicator = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("VoidShopPortalIndicator", SS2Bundle.Indev), ti.modelChildLocator.FindChild("MSPortalIndicator").parent);
            }

            //update teleporter holdout zone / directors / etc.
            hzc.baseRadius *= 1.5f;
            hzc.baseChargeDuration *= 1.25f;
            var cd = ti.bonusDirector;
            cd.maxConsecutiveCheapSkips = 2; //:slight_smile:
            cd.maximumNumberToSpawnBeforeSkipping = 10;

            //update the fresnel material from red to green
            var newTeleMat = SS2Assets.LoadAsset<Material>("matEtherealFresnelOverlay", SS2Bundle.Indev);
            ReplaceMaterial(teleBase.gameObject.GetComponent<MeshRenderer>(), 1, newTeleMat);
            ReplaceMaterial(teleBase.Find("TeleporterProngMesh").gameObject.GetComponent<Renderer>(), 1, newTeleMat);
            ReplaceMaterial(teleBase.Find("SurfaceHeight/TeleporterBeacon").gameObject.GetComponent<Renderer>(), 1, newTeleMat);
            void ReplaceMaterial(Renderer renderer, int index, Material material)
            {
                Material[] materials = renderer.sharedMaterials;
                materials[index] = material;
                renderer.sharedMaterials = materials;
            }
            //resize & reposition the teleporter
            //teleBase.transform.localScale *= 2f;
            //teleBase.transform.position = new Vector3(teleBase.transform.position.x, teleBase.transform.position.y, teleBase.transform.position.z);
            //teleProngs.transform.localScale *= 2f;
            //teleBeacon.transform.localScale *= 0.5f;
            //radiusScaler.transform.localScale *= 0.67f;

            //resize & recolor large teleporter particles
            var tppMain = telePassiveParticles.main;
            telePassiveParticles.transform.localScale *= 1.5f;
            tppMain.startColor = new Color(0f, .89f, .39f);

            //resize & recolor particles above teleporter well
            var tcpMain = teleCenterParticles.main;
            tcpMain.startColor = new Color(.0f, .89f, .39f);

            //re-material the range indicator
            rangeIndicator.material = SS2Assets.LoadAsset<Material>("matEthTeleporterRangeIndicator", SS2Bundle.Indev);

            //lightning effects
            lightningParticles.GetComponent<ParticleSystemRenderer>().material = SS2Assets.LoadAsset<Material>("matEthTPLightning", SS2Bundle.Indev);
            lightningLightRef.color = new Color(.0f, .89f, .39f);

            //idletocharging
            betweenProngsCore.GetComponent<ParticleSystemRenderer>().material = SS2Assets.LoadAsset<Material>("matEthTPFire", SS2Bundle.Indev);
            coreLightRef.color = new Color(.0f, .89f, .39f);

            //3DDebris
            debris.GetComponent<ParticleSystemRenderer>().trailMaterial = SS2Assets.LoadAsset<Material>("matEthExplosion", SS2Bundle.Indev);

            //charging effects
            chargingRing.GetComponent<ParticleSystemRenderer>().material = SS2Assets.LoadAsset<Material>("matEthTPShockwave", SS2Bundle.Indev);

            //center beam
            loopLight.color = new Color(.0f, .89f, .39f);
            var beamCorePSR = core.GetComponent<ParticleSystemRenderer>();
            beamCorePSR.material = SS2Assets.LoadAsset<Material>("matEthTPFire", SS2Bundle.Indev);
            var beamPSR = beam.GetComponent<ParticleSystemRenderer>();
            beamPSR.material = SS2Assets.LoadAsset<Material>("matEthTPLaser", SS2Bundle.Indev);

            //indicator sprite
            //teleporterSprite.color = new Color(.0f, .89f, .39f);

            //there's so many vfx to replace and recolor.
            //it'd maybe be easier to make a new prefab entirely although i can't help but feel it makes more sense to just edit the existing...
            //maybe particle system prefabs in unity & code to adjust scale / pos of the objects?
            //hell either way.


            // -- i think its just the effects for when its finished charging at this point though? other than like hud stuff n other weird shit. yippee
        }
    }
}
