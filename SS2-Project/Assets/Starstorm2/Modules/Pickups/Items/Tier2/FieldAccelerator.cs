using SS2.Orbs;
using R2API;
using RoR2;
using RoR2.Items;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
    public sealed class FieldAccelerator : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acFieldAccelerator", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of charge to add to the teleporter on kill. (1 = 100%)")]
        [FormatToken("SS2_ITEM_FIELDACCELERATOR_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float chargePerKill = 0.01f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Extra % teleporter charge radius. (0.01 = 1%")]
        [FormatToken("SS2_ITEM_FIELDACCELERATOR_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float radiusPerStack = 0.75f;

        private static GameObject _objectPrefab;

        public override void Initialize()
        {
            //???
            _objectPrefab = AssetCollection.FindAsset<GameObject>("ObjectFieldAccelerator"); // 
            _objectPrefab = PrefabAPI.InstantiateClone(SS2Assets.LoadAsset<GameObject>("ObjectFieldAccelerator", SS2Bundle.Items), "ObjectDisplayFieldAccelerator", true);
            _objectPrefab.RegisterNetworkPrefab();

            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/Teleporter1.prefab").Completed+= (r) => r.Result.AddComponent<FieldAcceleratorTeleporterBehavior>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/LunarTeleporter Variant.prefab").Completed += (r) => r.Result.AddComponent<FieldAcceleratorTeleporterBehavior>();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.FieldAccelerator;

            private HurtBox displayHurtbox;
            private GameObject displayInstance;
            public void OnKilledOtherServer(DamageReport damageReport)
            {
                var teleInstance = TeleporterInteraction.instance;
                if (teleInstance && teleInstance.isCharging)
                {
                    bool flag = damageReport.attackerBody == body;
                    bool flag1 = teleInstance.activationState == TeleporterInteraction.ActivationState.Charging;
                    bool flag2 = teleInstance.monstersCleared;
                    bool flag3 = teleInstance.holdoutZoneController.IsBodyInChargingRadius(damageReport.victimBody);
                    if (flag && flag1 && flag2 && flag3)
                    {
                        if (displayHurtbox != null)
                        {
                            FieldAcceleratorOrb orb = new FieldAcceleratorOrb();
                            orb.origin = damageReport.victimBody.transform.position;
                            orb.target = displayHurtbox;
                            OrbManager.instance.AddOrb(orb);
                        }
                        else
                        {
                            teleInstance.holdoutZoneController.charge += chargePerKill;
                            //Util.PlaySound("AcceleratorAddCharge", teleInstance.gameObject);
                        }
                    }
                }
            }

            private void TeleporterInteraction_onTeleporterBeginChargingGlobal(TeleporterInteraction tele)
            {
                if (displayHurtbox == null)
                {
                    var teleInstance = TeleporterInteraction.instance;
                    FieldAcceleratorTeleporterBehavior fatb = teleInstance.GetComponent<FieldAcceleratorTeleporterBehavior>();
                    displayInstance = fatb.displayInstance;
                    if (displayInstance != null)
                        displayHurtbox = displayInstance.GetComponent<HurtBoxGroup>().mainHurtBox;
                }
            }

            private void Start()
            {
                TeleporterInteraction.onTeleporterBeginChargingGlobal += TeleporterInteraction_onTeleporterBeginChargingGlobal;
                var teleInstance = TeleporterInteraction.instance;

                if (teleInstance)
                {
                    if (teleInstance.GetComponent<FieldAcceleratorTeleporterBehavior>() == null)
                        teleInstance.gameObject.AddComponent<FieldAcceleratorTeleporterBehavior>();

                    FieldAcceleratorTeleporterBehavior fatb = teleInstance.GetComponent<FieldAcceleratorTeleporterBehavior>();

                    fatb.RecalcRadius();

                    displayInstance = fatb.displayInstance;
                    if (displayInstance != null)
                        displayHurtbox = displayInstance.GetComponent<HurtBoxGroup>().mainHurtBox;
                }
            }

            private void OnDestroy()
            {
                TeleporterInteraction.onTeleporterBeginChargingGlobal -= TeleporterInteraction_onTeleporterBeginChargingGlobal;

                var teleInstance = TeleporterInteraction.instance;

                if (teleInstance)
                {
                    FieldAcceleratorTeleporterBehavior fatb = teleInstance.GetComponent<FieldAcceleratorTeleporterBehavior>();
                    if (fatb != null)
                        fatb.RecalcRadius();
                }
            }
        }

        public class FieldAcceleratorTeleporterBehavior : NetworkBehaviour
        {
            private HoldoutZoneController hzc;
            private Transform pos;
            public static GameObject displayPrefab;
            private ChildLocator displayChildLocator;
            public static float acceleratorCount;
            private bool teleCharging;
            private bool monstersCleared;
            public GameObject displayInstance;
            private float timer;

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

            private void Awake()
            {
                hzc = GetComponent<HoldoutZoneController>();
                TeleporterInteraction.onTeleporterBeginChargingGlobal += TeleporterInteraction_onTeleporterBeginChargingGlobal;
                TeleporterInteraction.onTeleporterChargedGlobal += TeleporterInteraction_onTeleporterChargedGlobal;
                RecalcRadius();
                teleCharging = false;
                monstersCleared = false;
            }

            private void TeleporterInteraction_onTeleporterBeginChargingGlobal(TeleporterInteraction tele)
            {
                //RecalcRadius();
                teleCharging = true;
            }

            private void TeleporterInteraction_onTeleporterChargedGlobal(TeleporterInteraction tele)
            {
                acceleratorCount = 0;

                foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController.instances)
                {
                    if (pcmc.master && pcmc.master.inventory)
                        acceleratorCount += pcmc.master.inventory.GetItemCount(SS2Content.Items.FieldAccelerator);
                }


                if (acceleratorCount == 0)
                    return;

                if (displayChildLocator == null)
                {
                    displayChildLocator = displayInstance.GetComponent<ChildLocator>();
                }

                if (displayChildLocator != null)
                {
                    displayChildLocator.FindChild("Passive").gameObject.SetActive(false);
                    displayChildLocator.FindChild("Burst").gameObject.GetComponent<ParticleSystem>().Emit(40);
                    displayChildLocator.FindChild("Ring").gameObject.GetComponent<ParticleSystem>().Emit(1);
                    //Util.PlaySound("AcceleratorBoot", displayInstance.gameObject);
                }
            }

            private void FixedUpdate()
            {
                timer += Time.fixedDeltaTime;
                if (timer > 0.5 && acceleratorCount > 0)
                {
                    timer = 0;

                    if (hzc == null)
                    {
                        hzc = GetComponent<HoldoutZoneController>();
                    }

                    if (teleCharging == true && monstersCleared == false)
                    {
                        if (TeleporterInteraction.instance.monstersCleared == true)
                        {
                            monstersCleared = true;
                            Debug.Log(monstersCleared + " : monsters cleared");
                            RecalcRadius();
                            UpgradeTeleporterRadius();
                        }
                    }
                }
            }

            private void UpgradeTeleporterRadius()
            {
                //Debug.Log("RIP sleep in OU 2024");

                acceleratorCount = 0;

                foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController.instances)
                {
                    if (pcmc.master && pcmc.master.inventory)
                        acceleratorCount += pcmc.master.inventory.GetItemCount(SS2Content.Items.FieldAccelerator);
                }

                if (acceleratorCount == 0)
                    return;

                //set a bunch of variables we'll be using for teleporter modifications:

                //get EVERYTHING
                telePassiveParticles = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/PassiveParticle, Sphere").GetComponent<ParticleSystem>();
                teleCenterParticles = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/PassiveParticle, Center").GetComponent<ParticleSystem>();
                lightningParticles = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargedEffect/LightningAlongProngs").GetComponent<ParticleSystem>();
                lightningLightRef = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargedEffect/LightningAlongProngs/ReferencePointLight").GetComponent<Light>();
                betweenProngsCore = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/IdleToChargingEffect/BetweenProngs/Core").GetComponent<ParticleSystem>();
                coreLightRef = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/IdleToChargingEffect/BetweenProngs/Point light").GetComponent<Light>();
                debris = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/IdleToChargingEffect/3DDebris").GetComponent<ParticleSystem>();
                chargingRing = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargingEffect/Ring").GetComponent<ParticleSystem>();
                loopLight = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargingEffect/BetweenProngs/Loop/Point light").GetComponent<Light>();
                core = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargingEffect/BetweenProngs/Loop/Core").GetComponent<ParticleSystem>();
                beam = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargingEffect/BetweenProngs/Loop/Beam").GetComponent<ParticleSystem>();
                rangeIndicator = hzc.radiusIndicator;

                //resize & recolor large teleporter particles
                var tppMain = telePassiveParticles.main;
                tppMain.startColor = new Color(0.8f, .42f, 0f);

                //resize & recolor particles above teleporter well
                /*var tcpMain = teleCenterParticles.main;
                tcpMain.startColor = new Color(0.8f, .42f, 0f);*/

                //re-material the range indicator
                rangeIndicator.material = SS2Assets.LoadAsset<Material>("matAcceleratorTeleporterRangeIndicator", SS2Bundle.Items);

                //lightning effects
                /*var lightningPSR = lightningParticles.GetComponent<ParticleSystemRenderer>();
                lightningPSR.material = SS2Assets.LoadAsset<Material>("matAcceleratorTPLightning", SS2Bundle.Items);
                lightningLightRef.color = new Color(0.8f, .42f, 0f);*/

                //idletocharging
                var corePSR = betweenProngsCore.GetComponent<ParticleSystemRenderer>();
                corePSR.material = SS2Assets.LoadAsset<Material>("matAcceleratorTPFire", SS2Bundle.Items);
                coreLightRef.color = new Color(0.8f, .42f, 0f);

                //3DDebris
                var debrisPSR = debris.GetComponent<ParticleSystemRenderer>();
                debrisPSR.trailMaterial = SS2Assets.LoadAsset<Material>("matAcceleratorExplosion", SS2Bundle.Items);

                //charging effects
                var ringPSR = chargingRing.GetComponent<ParticleSystemRenderer>();
                ringPSR.material = SS2Assets.LoadAsset<Material>("matAcceleratorTPShockwave", SS2Bundle.Items);

                //center beam
                loopLight.color = new Color(0.8f, .42f, 0f);
                var beamCorePSR = core.GetComponent<ParticleSystemRenderer>();
                beamCorePSR.material = SS2Assets.LoadAsset<Material>("matAcceleratorTPFire", SS2Bundle.Items);
                //core.transform.position += new Vector3(core.transform.position.x, core.transform.position.y + 1.5f, core.transform.position.z);
                var beamPSR = beam.GetComponent<ParticleSystemRenderer>();
                beamPSR.material = SS2Assets.LoadAsset<Material>("matAcceleratorTPLaser", SS2Bundle.Items);
                //beam.transform.position += new Vector3(beam.transform.position.x, beam.transform.position.y + 1.5f, beam.transform.position.z);

                displayChildLocator.FindChild("Passive").gameObject.SetActive(true);
                displayChildLocator.FindChild("Burst").gameObject.GetComponent<ParticleSystem>().Emit(40);
                displayChildLocator.FindChild("Ring").gameObject.GetComponent<ParticleSystem>().Emit(1);
            }

            public void RecalcRadius()
            {
                acceleratorCount = 0;

                if (hzc == null)
                    hzc = GetComponent<HoldoutZoneController>();

                foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController.instances)
                {
                    if (pcmc.master && pcmc.master.inventory)
                        acceleratorCount += pcmc.master.inventory.GetItemCount(SS2Content.Items.FieldAccelerator);
                }

                if (NetworkServer.active)
                {
                    if (acceleratorCount > 0 && displayInstance == null)
                    {
                        Vector3 position = new Vector3(TeleporterInteraction.instance.transform.position.x, TeleporterInteraction.instance.transform.position.y + 1.5f, TeleporterInteraction.instance.transform.position.z);
                        displayInstance = Instantiate(_objectPrefab, position, new Quaternion(0, 0, 0, 0));
                        displayChildLocator = displayInstance.GetComponent<ChildLocator>();
                        NetworkServer.Spawn(displayInstance);
                    }

                    if (acceleratorCount == 0 && displayInstance != null)
                    {
                        NetworkServer.Destroy(displayInstance);
                    }
                }
                

                if (hzc != null && acceleratorCount > 0 && monstersCleared)
                {
                    hzc.baseRadius *= 1 + (radiusPerStack * acceleratorCount);
                }
            }
        }
    }
}
