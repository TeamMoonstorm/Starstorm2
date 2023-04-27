using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using R2API;
using RoR2;
using System.Text;
using R2API.Utils;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2
{
    class Ethereal
    {
        public static Ethereal instance { get; private set; }
        public static Color32 ethColor = new Color32(18, 93, 74, 255);

        public static int etherealsCompleted { get; private set; }
        public static bool teleIsEthereal;
        private static float storedScalingValue;

        public static Material teleporterFresnel;

        public static bool teleUpgraded;
        internal static void Init()
        {
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            TeleporterInteraction.onTeleporterBeginChargingGlobal += TeleporterInteraction_onTeleporterBeginChargingGlobal;
            TeleporterInteraction.onTeleporterFinishGlobal += TeleporterInteraction_onTeleporterFinishGlobal;
            On.RoR2.TeleporterInteraction.FixedUpdate += TeleporterInteraction_FixedUpdate;
            On.RoR2.TeleporterInteraction.Start += TeleporterInteraction_Start;
            Run.

            teleporterFresnel = Addressables.LoadAssetAsync<Material>("RoR2/Base/Teleporters/matTeleporterFresnelOverlay.mat").WaitForCompletion();
        }

        private static void Run_CCRunEnd(Run run)
        {

        }

        private static void Run_onRunStartGlobal(Run run)
        {
            Debug.Log("Setting up Ethereals..");
            var diffToken = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).nameToken;
            var builder = new StringBuilder();
            builder.Append(Language.GetString(diffToken));
            builder.Append(" +1");
            storedScalingValue = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue;
            Debug.Log(DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue + " - scaling value");
            etherealsCompleted = 0;
            teleIsEthereal = false;
            Debug.Log("completed: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);
        }

        private static void Run_onRunDestroyGlobal(Run run)
        {
            DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue = storedScalingValue;
            Debug.Log(DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue + " - scaling value");
            Debug.Log("completed: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);
        }

        private static void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);

            if (self.teleporterInstance)
            {
                var position = Vector3.zero;
                var rotation = Quaternion.Euler(-90, 0, 0);
                var currStage = SceneManager.GetActiveScene().name;
                switch (currStage)
                {
                    case "blackbeach":
                        position = new Vector3(-60, -51.2f, -231);
                        rotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case "blackbeach2":
                        position = new Vector3(-101.4f, 1.5f, 20.1f);
                        rotation = Quaternion.Euler(0, 292.8f, 0);
                        break;
                    case "golemplains":
                        position = new Vector3(283.7f, -50.0f, -154.7f);
                        rotation = Quaternion.Euler(0, 321, 0);
                        break;
                    case "golemplains2":
                        position = new Vector3(9.8f, 127.5f, -251.8f);
                        rotation = Quaternion.Euler(0, 5, 0);
                        break;
                    case "goolake":
                        position = new Vector3(53.9f, -45.9f, -219.6f);
                        rotation = Quaternion.Euler(0, 190, 0);
                        break;
                    case "foggyswamp":
                        position = new Vector3(-83.74f, -83.35f, 39.09f);
                        rotation = Quaternion.Euler(0, 104.27f, 0);
                        break;
                    case "frozenwall":
                        position = new Vector3(-230.7f, 132, 239.4f);
                        rotation = Quaternion.Euler(0, 167, 0);
                        break;
                    case "wispgraveyard":
                        position = new Vector3(-341.5f, 79, 0.5f);
                        rotation = Quaternion.Euler(0, 145, 0);
                        break;
                    case "dampcavesimple":
                        position = new Vector3(157.5f, -43.1f, -188.9f);
                        rotation = Quaternion.Euler(0, 318.4f, 0);
                        break;
                    case "shipgraveyard":
                        position = new Vector3(20.5f, -23.7f, 185.1f);
                        rotation = Quaternion.Euler(0, 173.6f, 0);
                        break;
                    case "rootjungle":
                        position = new Vector3(-196.6f, 190.1f, -204.5f);
                        rotation = Quaternion.Euler(0, 80, 0);
                        break;
                    case "skymeadow":
                        position = new Vector3(65.9f, 127.4f, -293.9f);
                        rotation = Quaternion.Euler(0, 194.8f, 0);
                        break;
                    case "snowyforest":
                        position = new Vector3(-38.7f, 112.7f, 153.1f);
                        rotation = Quaternion.Euler(0, 54.1f, 0);
                        break;
                    case "ancientloft":
                        position = new Vector3(-133.4f, 33.5f, -280f);
                        rotation = Quaternion.Euler(0, 354.5f, 0);
                        break;
                    case "sulfurpools":
                        position = new Vector3(-33.6f, 36.8f, 164.1f);
                        rotation = Quaternion.Euler(0, 187f, 0);
                        break;
                }
                var term = Object.Instantiate(SS2Assets.LoadAsset<GameObject>("ShrineEthereal", SS2Bundle.Indev), position, rotation);
            }

            Debug.Log("completed ethereals: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);
            if (teleIsEthereal)
            {
                ChatMessage.Send("SS2_ETHEREAL_DIFFICULTY_WARNING");
                teleIsEthereal = false;
                etherealsCompleted++;
                //update difficulty
                var run = Run.instance;
                if (NetworkServer.active && run)
                {
                    //one-time difficulty adjustments
                    DifficultyDef curDiff = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty);
                    switch (curDiff.scalingValue)
                    {
                        case 1:
                            foreach (CharacterMaster cm in run.userMasters.Values)
                            {
                                if (cm.inventory)
                                    cm.inventory.RemoveItem(RoR2Content.Items.DrizzlePlayerHelper.itemIndex);
                            }
                            break;
                        case 2:
                            foreach (CharacterMaster cm in run.userMasters.Values)
                            {
                                if (cm.inventory)
                                    cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);
                            }
                            break;
                        case 3:
                            {
                                TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit *= 2;
                                TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit *= 2;
                                TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit *= 2;
                            }
                            break;
                    }

                    //drizzle -> rainstorm, rainstorm -> monsoon
                    if (curDiff.scalingValue < 3)
                        curDiff.scalingValue += 1;
                    //monsoon -> typhoon, typhoon +25%
                    else
                    {
                        curDiff.scalingValue += 0.5f;
                    }

                    if (run.selectedDifficulty == Typhoon.TyphoonIndex)
                        run.selectedDifficulty = SuperTyphoon.SuperTyphoonIndex;
        
                    string diffToken = curDiff.nameToken;
                    Debug.Log(DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue + " - current scaling value");
                    Debug.Log("completed: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);
                }
            }
        }

        private static void TeleporterInteraction_Start(On.RoR2.TeleporterInteraction.orig_Start orig, TeleporterInteraction self)
        {
            orig(self);
            {
                var newTeleMat = teleporterFresnel;
                var teleBase = self.gameObject.transform.Find("TeleporterBaseMesh").gameObject;
                var teleProngs = teleBase.transform.Find("TeleporterProngMesh").gameObject;
                var teleBeacon = teleBase.transform.Find("SurfaceHeight").Find("TeleporterBeacon").gameObject;
                teleBase.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                teleProngs.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                teleBeacon.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
            }
            
        }

        private static void TeleporterInteraction_FixedUpdate(On.RoR2.TeleporterInteraction.orig_FixedUpdate orig, TeleporterInteraction self)
        {
            orig(self);

            if (teleIsEthereal && !teleUpgraded)
            {
                Debug.Log("tele change");
                teleUpgraded = true;
                var newTeleMat = SS2Assets.LoadAsset<Material>("matEtherealFresnelOverlay", SS2Bundle.Indev);
                var teleBase = self.gameObject.transform.Find("TeleporterBaseMesh").gameObject;
                var teleProngs = teleBase.transform.Find("TeleporterProngMesh").gameObject;
                var teleBeacon = teleBase.transform.Find("SurfaceHeight").Find("TeleporterBeacon").gameObject;
                var teleParticleSphere = teleBase.transform.Find("BuiltInEffects").Find("PassiveParticle, Sphere").gameObject;
                var teleParticleCenter = teleBase.transform.Find("BuiltInEffects").Find("PassiveParticle, Center").gameObject;
                teleBase.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                teleProngs.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                teleBeacon.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                teleBase.transform.localScale *= 1.5f;
                //teleProngs.transform.localScale *= 2f;
                teleBeacon.transform.localScale *= 0.5f;

                ParticleSystem telePassiveParticles = teleParticleSphere.GetComponent<ParticleSystem>();
                teleParticleSphere.transform.localScale *= 2f;
                telePassiveParticles.startColor = new Color(.8f, .32f, .39f);

                ParticleSystem teleCenterParticles = teleParticleCenter.GetComponent<ParticleSystem>();
                teleCenterParticles.startColor = new Color(.8f, .32f, .39f);

                //there's so many vfx to replace and recolor.
                //hell.
            }
            if (!teleIsEthereal && teleUpgraded)
                teleUpgraded = false;
        }

        private static void TeleporterInteraction_onTeleporterBeginChargingGlobal(TeleporterInteraction tele)
        {
            if (teleIsEthereal)
            {
                if (NetworkServer.active)
                {
                    TeleporterInteraction.instance.holdoutZoneController.calcRadius += HoldoutZoneController_calcRadius;
                    TeleporterInteraction.instance.holdoutZoneController.calcChargeRate += HoldoutZoneController_calcChargeRate;

                    Debug.Log("completed: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);

                    if (tele.bossDirector)
                    {
                        tele.bossDirector.monsterCredit += (float)(int)(600f * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f));
                        Debug.Log("added to monstercred");
                    }
                    if (tele.bonusDirector)
                    {
                        tele.bonusDirector.monsterCredit += (float)(int)(600f * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f));
                        Debug.Log("added to bonus monstercred");
                    }
                }
            }
        }

        private static void HoldoutZoneController_calcRadius(ref float radius)
        {
            if (teleIsEthereal)
            {
                radius *= 1.5f;
            }
        }

        private static void HoldoutZoneController_calcChargeRate(ref float rate)
        {
            if (teleIsEthereal)
            {
                rate *= 0.75f;
            }
        }

        private static void TeleporterInteraction_onTeleporterFinishGlobal(TeleporterInteraction obj)
        { }
    }
    /*public class ShrineEtherealBehavior : NetworkBehaviour
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
            /*purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.onPurchase.AddListener(ActivateEtherealTerminal);
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
    }*/
}


