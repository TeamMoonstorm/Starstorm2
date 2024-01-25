using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class FieldAccelerator : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("FieldAccelerator", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of charge to add to the teleporter on kill. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_FIELDACCELERATOR_DESC", StatTypes.MultiplyByN, 0, "100")]
        public static float chargePerKill = 0.01f;

        /*[RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Maximum Amount of Charge per kill. (1 = 100%)")]
        public static float maxChargePerKill = 0.05f;*/

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Extra % teleporter charge radius. (0.01 = 1%")]
        [TokenModifier("SS2_ITEM_FIELDACCELERATOR_DESC", StatTypes.MultiplyByN, 1, "100")]
        public static float radiusPerStack = 15f;

        public static GameObject objectPrefab;

        public override void Initialize()
        {
            base.Initialize();
            objectPrefab = PrefabAPI.InstantiateClone(SS2Assets.LoadAsset<GameObject>("ObjectFieldAccelerator", SS2Bundle.Items), "ObjectDisplayFieldAccelerator", true);
            objectPrefab.RegisterNetworkPrefab();
            //Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/Teleporter1.prefab").WaitForCompletion().AddComponent<FieldAcceleratorTeleporterBehavior>();
            //Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/LunarTeleporter Variant.prefab").WaitForCompletion().AddComponent<FieldAcceleratorTeleporterBehavior>();
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.FieldAccelerator;
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
                        teleInstance.holdoutZoneController.charge += chargePerKill;
                    //Holy fuck since when has .InverseHyperbolicScaling existed???
                    //Since not too long ago, i stolñe it from komrade, lmao
                    //gone.
                }
            }

            private void Start()
            {
                var teleInstance = TeleporterInteraction.instance;
                if (teleInstance)
                {
                    teleInstance.gameObject.AddComponent<FieldAcceleratorTeleporterBehavior>();
                }
            }
        }

        public class FieldAcceleratorTeleporterBehavior : NetworkBehaviour
        {
            private HoldoutZoneController hzc;
            private Transform pos;
            public static GameObject displayPrefab;
            public static float acceleratorCount;
            private bool teleCharging;
            private bool monstersCleared;
            public GameObject displayInstance;
            private float timer;

            private void Awake()
            {
                hzc = GetComponent<HoldoutZoneController>();
                TeleporterInteraction.onTeleporterBeginChargingGlobal += TeleporterInteraction_onTeleporterBeginChargingGlobal;
                RecalcRadius();
                teleCharging = false;
                monstersCleared = false;
            }

            private void TeleporterInteraction_onTeleporterBeginChargingGlobal(TeleporterInteraction tele)
            {
                RecalcRadius();
                teleCharging = true;
            }

            private void FixedUpdate()
            {
                timer += Time.fixedDeltaTime;
                if (timer > 0.5)
                {
                    timer = 0;
                    if (hzc == null)
                        hzc = GetComponent<HoldoutZoneController>();

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
                //visual stuff...
                //make teleporter radius / beam orange? there's so many fcuking vfx.

                //like fucking LOOK AT THIS:

                /*//set a bunch of variables we'll be using for teleporter modifications:
                var newTeleMat = SS2Assets.LoadAsset<Material>("matEtherealFresnelOverlay", SS2Bundle.Indev);
                var teleBase = GameObject.Find("TeleporterBaseMesh").gameObject;
                var teleProngs = teleBase.transform.Find("TeleporterProngMesh").gameObject;
                var teleBeacon = teleBase.transform.Find("SurfaceHeight/TeleporterBeacon").gameObject;
                var teleBuiltInEffects = teleBase.transform.Find("BuiltInEffects").gameObject;
                var radiusScaler = teleBuiltInEffects.transform.Find("ChargingEffect/RadiusScaler").gameObject;

                //update the fresnel material from red to green
                if (currStage != "skymeadow")
                {
                    teleBase.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                    teleProngs.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                    teleBeacon.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                }

                //resize & reposition the teleporter
                teleBase.transform.localScale *= 1f;
                teleBase.transform.position = new Vector3(teleBase.transform.position.x, teleBase.transform.position.y, teleBase.transform.position.z);
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
                var lightningPSR = lightningParticles.GetComponent<ParticleSystemRenderer>();
                lightningPSR.material = SS2Assets.LoadAsset<Material>("matEthTPLightning", SS2Bundle.Indev);
                lightningLightRef.color = new Color(.0f, .89f, .39f);

                //idletocharging
                var corePSR = betweenProngsCore.GetComponent<ParticleSystemRenderer>();
                corePSR.material = SS2Assets.LoadAsset<Material>("matEthTPFire", SS2Bundle.Indev);
                coreLightRef.color = new Color(.0f, .89f, .39f);

                //3DDebris
                var debrisPSR = debris.GetComponent<ParticleSystemRenderer>();
                debrisPSR.trailMaterial = SS2Assets.LoadAsset<Material>("matEthExplosion", SS2Bundle.Indev);

                //charging effects
                var ringPSR = chargingRing.GetComponent<ParticleSystemRenderer>();
                ringPSR.material = SS2Assets.LoadAsset<Material>("matEthTPShockwave", SS2Bundle.Indev);

                //center beam
                loopLight.color = new Color(.0f, .89f, .39f);
                var beamCorePSR = core.GetComponent<ParticleSystemRenderer>();
                beamCorePSR.material = SS2Assets.LoadAsset<Material>("matEthTPFire", SS2Bundle.Indev);
                var beamPSR = beam.GetComponent<ParticleSystemRenderer>();
                beamPSR.material = SS2Assets.LoadAsset<Material>("matEthTPLaser", SS2Bundle.Indev);*/
            }

            private void RecalcRadius()
            {
                acceleratorCount = 0;

                if (hzc == null)
                    hzc = GetComponent<HoldoutZoneController>();

                foreach (CharacterMaster cm in Run.instance.userMasters.Values)
                {
                    if (cm.inventory)
                        acceleratorCount += cm.inventory.GetItemCount(SS2Content.Items.FieldAccelerator);
                }

                if (acceleratorCount > 0 && displayInstance == null)
                {
                    displayInstance = Instantiate(objectPrefab, TeleporterInteraction.instance.transform.position, new Quaternion(0, 0, 0, 0));
                    NetworkServer.Spawn(displayInstance);
                }

                if (!monstersCleared)
                    return;

                hzc.baseRadius *= 1 + (radiusPerStack * acceleratorCount);
                hzc.currentRadius *= 1 + (radiusPerStack * acceleratorCount);
            }
        }
    }
}
