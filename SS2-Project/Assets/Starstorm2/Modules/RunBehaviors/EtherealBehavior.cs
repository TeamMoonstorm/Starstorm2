﻿using R2API;
using R2API.ScriptableObjects;
using R2API.Utils;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
namespace SS2.Components
{
    //if this code looks stupid in any form of the word, please let me know -★
    public class EtherealBehavior : MonoBehaviour
    {
        public static EtherealBehavior instance { get; private set; }
        private static float storedScalingValue;
        private static int storedLevelCap;
        public Run run;

        public static Dictionary<DifficultyIndex, SerializableDifficultyDef> diffDicts = new Dictionary<DifficultyIndex, SerializableDifficultyDef>();

        public static GameObject shrinePrefab;
        public static GameObject portalPrefab;

        public int etherealsCompleted = 0;
        public static bool teleIsEthereal = false;

        public bool teleUpgraded;

        public bool adversityEnabled;

        internal static IEnumerator Init()
        {
            //Initialize trader trading
            TraderController.Initialize();

            //Initialize new difficulties

            //N: These are done by a new module exclusive to ss2, refactor as needed
            /*Deluge.;
            Tempest.Init();
            Cyclone.Init();
            SuperTyphoon.Init();*/

            //Save default level cap
            storedLevelCap = Run.ambientLevelCap;

            //Create difficulty indicies dict
            CreateDifficultyDict();

            //Initialize related prefabs
            shrinePrefab = PrefabAPI.InstantiateClone(SS2Assets.LoadAsset<GameObject>("ShrineEthereal", SS2Bundle.Indev), "EtherealSapling", true);
            shrinePrefab.RegisterNetworkPrefab();
            portalPrefab = PrefabAPI.InstantiateClone(SS2Assets.LoadAsset<GameObject>("PortalStranger1", SS2Bundle.SharedStages), "StrangerPortal", true);
            portalPrefab.RegisterNetworkPrefab();

            //Add teleporter upgrading component to teleporters
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/Teleporter1.prefab").WaitForCompletion().AddComponent<TeleporterUpgradeController>();
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/LunarTeleporter Variant.prefab").WaitForCompletion().AddComponent<TeleporterUpgradeController>();

            yield return null;
        }

        private void Awake()
        {
            run = GetComponentInParent<Run>();
        }

        private static void CreateDifficultyDict()
        {
            diffDicts.Add(DifficultyIndex.Easy, Deluge.sdd);
            diffDicts.Add(Deluge.sdd.DifficultyIndex, Deluge1.sdd);
            diffDicts.Add(Deluge1.sdd.DifficultyIndex, Deluge2.sdd);
            diffDicts.Add(Deluge2.sdd.DifficultyIndex, Deluge3.sdd);

            diffDicts.Add(DifficultyIndex.Normal, Tempest.sdd);
            diffDicts.Add(Tempest.sdd.DifficultyIndex, Tempest1.sdd);
            diffDicts.Add(Tempest1.sdd.DifficultyIndex, Tempest2.sdd);
            diffDicts.Add(Tempest2.sdd.DifficultyIndex, Tempest3.sdd);

            diffDicts.Add(DifficultyIndex.Hard, Cyclone.sdd);
            diffDicts.Add(Cyclone.sdd.DifficultyIndex, Cyclone1.sdd);
            diffDicts.Add(Cyclone1.sdd.DifficultyIndex, Cyclone2.sdd);
            diffDicts.Add(Cyclone2.sdd.DifficultyIndex, Cyclone3.sdd);

            diffDicts.Add(Typhoon.sdd.DifficultyIndex, SuperTyphoon.sdd);
            diffDicts.Add(SuperTyphoon.sdd.DifficultyIndex, SuperTyphoon1.sdd);
            diffDicts.Add(SuperTyphoon1.sdd.DifficultyIndex, SuperTyphoon2.sdd);
            diffDicts.Add(SuperTyphoon2.sdd.DifficultyIndex, SuperTyphoon3.sdd);
        }

        private void Start()
        {
            instance = this;

            etherealsCompleted = 0;
            storedScalingValue = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue;
            teleIsEthereal = false;
            Run.ambientLevelCap = storedLevelCap;

            //TeleporterInteraction.onTeleporterBeginChargingGlobal += TeleporterInteraction_onTeleporterBeginChargingGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
            TeleporterInteraction.onTeleporterChargedGlobal += TeleporterInteraction_onTeleporterChargedGlobal;
            On.RoR2.TeleporterInteraction.Start += TeleporterInteraction_Start;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.TeleporterInteraction.OnBossDirectorSpawnedMonsterServer += TeleporterInteraction_OnBossDirectorSpawnedMonsterServer;
        }


        private void OnDestroy()
        {
            //TeleporterInteraction.onTeleporterBeginChargingGlobal -= TeleporterInteraction_onTeleporterBeginChargingGlobal;
            Run.onRunDestroyGlobal -= Run_onRunDestroyGlobal;
            TeleporterInteraction.onTeleporterChargedGlobal -= TeleporterInteraction_onTeleporterChargedGlobal;
            On.RoR2.TeleporterInteraction.Start -= TeleporterInteraction_Start;
            On.RoR2.SceneDirector.Start -= SceneDirector_Start;
            On.RoR2.TeleporterInteraction.OnBossDirectorSpawnedMonsterServer -= TeleporterInteraction_OnBossDirectorSpawnedMonsterServer;
        }

        private void Run_onRunDestroyGlobal(Run run)
        {
            DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue = storedScalingValue;
            Debug.Log(DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue + " - scaling value");
            Debug.Log("completed ethereals: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);
        }

        private void TeleporterInteraction_OnBossDirectorSpawnedMonsterServer(On.RoR2.TeleporterInteraction.orig_OnBossDirectorSpawnedMonsterServer orig, TeleporterInteraction self, GameObject master)
        {
            orig(self, master);
            if (teleIsEthereal)
            {
                CharacterMaster bodyMaster = master.GetComponent<CharacterMaster>();
                bodyMaster.inventory.GiveItem(SS2Content.Items.EtherealItemAffix);
                bodyMaster.inventory.GiveItem(RoR2Content.Items.BoostHp, (int)(12 + (6 * etherealsCompleted)));
            }
        }

        private static void TeleporterInteraction_onTeleporterChargedGlobal(TeleporterInteraction obj)
        {
            Debug.Log("tele finished");
            if (teleIsEthereal)
            {
                if (NetworkServer.active)
                {
                    Debug.Log("spawning portal");
                    var currStage = SceneManager.GetActiveScene().name;

                    var position = Vector3.zero;
                    var rotation = Quaternion.Euler(-90, 0, 0);

                    //add the ethereal terminal to a unique position per stage
                    //i hope you aren't here to cheat...
                    switch (currStage)
                    {
                        case "blackbeach":
                            position = new Vector3(-60, -47.2f, -231);
                            rotation = Quaternion.Euler(0, 0, 0);
                            //high cliff in the middle of the map
                            break;
                        case "blackbeach2":
                            position = new Vector3(-101.4f, 5.5f, 20.1f);
                            rotation = Quaternion.Euler(0, 292.8f, 0);
                            //between two knocked-over pillars near the gate
                            break;
                        case "golemplains":
                            position = new Vector3(283.7f, -46.0f, -154.7f);
                            rotation = Quaternion.Euler(0, 321, 0);
                            //top of the cliff, near debugging plains area
                            //home.
                            break;
                        case "golemplains2":
                            position = new Vector3(9.8f, 131.5f, -251.8f);
                            rotation = Quaternion.Euler(0, 5, 0);
                            //on the cliff where the middle giant ring meets the ground
                            break;
                        case "goolake":
                            position = new Vector3(53.9f, -41.9f, -219.6f);
                            rotation = Quaternion.Euler(0, 190, 0);
                            //on the clifftop near the ancient gate
                            break;
                        case "foggyswamp":
                            position = new Vector3(-83.74f, -79.8f, 39.09f);
                            rotation = Quaternion.Euler(0, 104.27f, 0);
                            //on the wall / dam across from where two newt altars spawn
                            break;
                        case "frozenwall":
                            position = new Vector3(-230.7f, 136, 239.4f);
                            rotation = Quaternion.Euler(0, 167, 0);
                            //on cliff near water, next to the lone tree
                            break;
                        case "wispgraveyard":
                            position = new Vector3(-341.5f, 83, 0.5f);
                            rotation = Quaternion.Euler(0, 145, 0);
                            //small cliff outcrop above playable area, same large island with artifact code
                            break;
                        case "dampcavesimple":
                            position = new Vector3(157.5f, -39.1f, -188.9f);
                            rotation = Quaternion.Euler(0, 318.4f, 0);
                            //on the overhang above rex w/ 3 big rocks
                            break;
                        case "shipgraveyard":
                            position = new Vector3(20.5f, -19.7f, 185.1f);
                            rotation = Quaternion.Euler(0, 173.6f, 0);
                            //in the cave entrance nearest to the cliff, on the spire below the land bridge
                            break;
                        case "rootjungle":
                            position = new Vector3(-196.6f, 190.1f, -204.5f);
                            rotation = Quaternion.Euler(0, 80, 0);
                            //top of the highest root in the upper / back area
                            break;
                        case "skymeadow":
                            position = new Vector3(65.9f, 127.4f, -293.9f);
                            rotation = Quaternion.Euler(0, 194.8f, 0);
                            //on top of the tallest rock spire, opposite side of map from the moon
                            break;
                        case "snowyforest":
                            position = new Vector3(-38.7f, 116.7f, 153.1f);
                            rotation = Quaternion.Euler(0, 54.1f, 0);
                            //on top of a lone elevated platform on a tree
                            break;
                        case "ancientloft":
                            position = new Vector3(-133.4f, 37.5f, -280f);
                            rotation = Quaternion.Euler(0, 354.5f, 0);
                            //on a branch under the main platform in the back corner of the map
                            break;
                        case "sulfurpools":
                            position = new Vector3(-33.6f, 40.8f, 164.1f);
                            rotation = Quaternion.Euler(0, 187f, 0);
                            //in the corner, atop of one of the columns
                            break;
                        case "FBLScene":
                            position = new Vector3(58.3f, 376f, -88.8f);
                            rotation = Quaternion.Euler(0, 0, 0);
                            //overlooking the shore
                            break;
                        case "drybasin":
                            position = new Vector3(149.4f, 69.7f, -212.7f);
                            rotation = Quaternion.Euler(0, 0, 0);
                            //in a cranny near collapsed aqueducts
                            break;
                        case "lakes":
                            position = new Vector3(-58.33718f, -28.5005f, -181.3314f);
                            rotation = Quaternion.Euler(0, 0, 0);
                            //behind a waterfall on the map's edge (how is there not already a secret here??)
                            break;
                    }

                    Debug.Log("POS : " + position);
                    Debug.Log("ROT : " + rotation);
                    Debug.Log("PORTALPREFAB : " + portalPrefab);
                    //CmdSpawnTerm(position, rotation);
                    GameObject portal = Instantiate(portalPrefab, position, rotation);
                    NetworkServer.Spawn(portal);
                    Debug.Log("TERM : " + portal);

                    Debug.Log("placed portal at: " + position + "pos & " + rotation + "rot");


                }
            }
        }

        private static void TeleporterInteraction_Start(On.RoR2.TeleporterInteraction.orig_Start orig, TeleporterInteraction self)
        {
            //on start: grab the teleporter model, and set the fresnel back to the regular red
            Debug.Log("setting tp back");
            var newTeleMat = SS2Assets.LoadAsset<Material>("matEtherealFresnelOverlayOG", SS2Bundle.Indev);
            var teleBase = self.gameObject.transform.Find("TeleporterBaseMesh").gameObject;
            var teleProngs = teleBase.transform.Find("TeleporterProngMesh").gameObject;
            var teleBeacon = teleBase.transform.Find("SurfaceHeight").Find("TeleporterBeacon").gameObject;
            teleBase.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
            teleProngs.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
            teleBeacon.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);

            orig(self);
        }

        private void TeleporterInteraction_onTeleporterBeginChargingGlobal(TeleporterInteraction tele)
        {
            if (teleIsEthereal)
            {
                if (NetworkServer.active)
                {
                    Debug.Log("ethereals completed: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);

                    if (tele.bossDirector)
                    {
                        //tele.bossDirector.monsterCredit += (float)(int)(100f * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f));
                        Debug.Log("added to monstercred");
                    }
                    if (tele.bonusDirector)
                    {
                        //tele.bonusDirector.monsterCredit += (float)(int)(50f * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f));
                        Debug.Log("added to bonus monstercred");
                    }
                }

            }
        }

        public void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);

            var currStage = SceneManager.GetActiveScene().name;

            if (self.teleporterInstance)
            {
                TeleporterUpgradeController tuc = self.teleporterInstance.GetComponent<TeleporterUpgradeController>();

                //if tuc exists & adversity doesn't force ethereal
                bool adversity = !RunArtifactManager.instance || (RunArtifactManager.instance && RunArtifactManager.instance.IsArtifactEnabled(SS2Content.Artifacts.Adversity));
                if (tuc != null && !((currStage == "skymeadow" || currStage == "slumbersatellite") && adversity))
                {
                    tuc.isEthereal = false;
                }

                var position = Vector3.zero;
                var rotation = Quaternion.Euler(-90, 0, 0);

                //big switch statement. i hope you aren't here to cheat.
                switch (currStage)
                {
                    case "blackbeach":
                        position = new Vector3(-60, -51.2f, -231);
                        rotation = Quaternion.Euler(0, 0, 0);
                        //high cliff in the middle of the map
                        break;
                    case "blackbeach2":
                        position = new Vector3(-101.4f, 1.5f, 20.1f);
                        rotation = Quaternion.Euler(0, 292.8f, 0);
                        //between two knocked-over pillars near the gate
                        break;
                    case "golemplains":
                        position = new Vector3(283.7f, -50.0f, -154.7f);
                        rotation = Quaternion.Euler(0, 321, 0);
                        //top of the cliff, near debugging plains area
                        //home.
                        break;
                    case "golemplains2":
                        position = new Vector3(9.8f, 127.5f, -251.8f);
                        rotation = Quaternion.Euler(0, 5, 0);
                        //on the cliff where the middle giant ring meets the ground
                        break;
                    case "goolake":
                        position = new Vector3(53.9f, -45.9f, -219.6f);
                        rotation = Quaternion.Euler(0, 190, 0);
                        //on the clifftop near the ancient gate
                        break;
                    case "foggyswamp":
                        position = new Vector3(-83.74f, -83.35f, 39.09f);
                        rotation = Quaternion.Euler(0, 104.27f, 0);
                        //on the wall / dam across from where two newt altars spawn
                        break;
                    case "frozenwall":
                        position = new Vector3(-230.7f, 132, 239.4f);
                        rotation = Quaternion.Euler(0, 167, 0);
                        //on cliff near water, next to the lone tree
                        break;
                    case "wispgraveyard":
                        position = new Vector3(-341.5f, 79, 0.5f);
                        rotation = Quaternion.Euler(0, 145, 0);
                        //small cliff outcrop above playable area, same large island with artifact code
                        break;
                    case "dampcavesimple":
                        position = new Vector3(157.5f, -43.1f, -188.9f);
                        rotation = Quaternion.Euler(0, 318.4f, 0);
                        //on the overhang above rex w/ 3 big rocks
                        break;
                    case "shipgraveyard":
                        position = new Vector3(20.5f, -23.7f, 185.1f);
                        rotation = Quaternion.Euler(0, 173.6f, 0);
                        //in the cave entrance nearest to the cliff, on the spire below the land bridge
                        break;
                    case "rootjungle":
                        position = new Vector3(-196.6f, 190.1f, -204.5f);
                        rotation = Quaternion.Euler(0, 80, 0);
                        //top of the highest root in the upper / back area
                        break;
                    /*case "skymeadow":
                        position = new Vector3(65.9f, 127.4f, -293.9f);
                        rotation = Quaternion.Euler(0, 194.8f, 0);
                        //on top of the tallest rock spire, opposite side of map from the moon
                        break;*/
                    case "snowyforest":
                        position = new Vector3(-38.7f, 112.7f, 153.1f);
                        rotation = Quaternion.Euler(0, 54.1f, 0);
                        //on top of a lone elevated platform on a tree
                        break;
                    case "ancientloft":
                        position = new Vector3(-133.4f, 33f, -280f);
                        rotation = Quaternion.Euler(0, 354.5f, 0);
                        //on a branch under the main platform in the back corner of the map
                        break;
                    case "sulfurpools":
                        position = new Vector3(-33.6f, 36.8f, 164.1f);
                        rotation = Quaternion.Euler(0, 187f, 0);
                        //in the corner, atop of one of the columns
                        break;
                    case "FBLScene":
                        position = new Vector3(58.3f, 372f, -88.8f);
                        rotation = Quaternion.Euler(0, 0, 0);
                        //overlooking the shore
                        break;
                    case "drybasin":
                        position = new Vector3(149.4f, 65.7f, -212.7f);
                        rotation = Quaternion.Euler(0, 0, 0);
                        //in a cranny near collapsed aqueducts
                        break;
                    case "lakes":
                        position = new Vector3(139f, 59.07873f, -181.3314f);
                        rotation = Quaternion.Euler(355f, 325f, 0);
                        //behind a waterfall on the map's edge (how is there not already a secret here??)
                        break;
                }

                if (NetworkServer.active)
                {
                    GameObject term = Instantiate(shrinePrefab, position, rotation);
                    NetworkServer.Spawn(term);
                }

                //this check should absolutely be refactored - is trying to ensure player progressed to next stage through regular teleporter
                //maybe move to when tp finishes charging?
                if (teleIsEthereal && (currStage != "artifactworld" && currStage != "arena" && currStage != "artifactworld" && currStage != "forgottenhaven"))
                {
                    ChatMessage.Send(Language.GetStringFormatted("SS2_ETHEREAL_DIFFICULTY_WARNING"));
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
                                    //remove drizzle helpers on drizzle to emulate rainstorm player scaling
                                    if (cm.inventory)
                                        cm.inventory.RemoveItem(RoR2Content.Items.DrizzlePlayerHelper.itemIndex);
                                }
                                break;
                            case 2:
                                foreach (CharacterMaster cm in run.userMasters.Values)
                                {
                                    //add monsoon helpers on rainstorm to emulate monsoon player scaling
                                    if (cm.inventory)
                                        cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);
                                }
                                break;
                            case 3:
                                {
                                    //increase team caps on monsoon to emulate typhoon enemy scaling
                                    TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit *= 2;
                                    TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit *= 2;
                                    TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit *= 2;
                                }
                                break;
                        }

                        //drizzle -> rainstorm, rainstorm -> monsoon
                        if (curDiff.scalingValue < 3)
                            curDiff.scalingValue += 1;
                        //monsoon -> typhoon, typhoon+ -> +25%
                        else
                        {
                            curDiff.scalingValue += 0.5f;
                        }

                        Run.ambientLevelCap += 100;

                        //update difficulty
                        if (NetworkServer.active)
                        {
                            run.RecalculateDifficultyCoefficent();

                            DifficultyIndex diffIndex = run.ruleBook.FindDifficulty();

                            Debug.Log("og difficulty diff: " + DifficultyCatalog.GetDifficultyDef(diffIndex).nameToken);

                            SerializableDifficultyDef newDiffDef;

                            for (int i = 0; i < run.ruleBook.ruleValues.Length; i++)
                            {
                                RuleChoiceDef ruleChoiceDef = run.ruleBook.GetRuleChoice(i);

                                diffDicts.TryGetValue(ruleChoiceDef.difficultyIndex, out newDiffDef);

                                run.selectedDifficulty = newDiffDef.DifficultyIndex;
                                run.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Difficulty." + Language.GetString(newDiffDef.nameToken)));

                                /*switch (ruleChoiceDef.difficultyIndex)
                                {
                                    //N: Sorry swuff, difficulties are now a module, need to refactor this as well, easy to do tbh, just store the serializable difficulty def in a static field and access that.
                                    //drizzle
                                    case DifficultyIndex.Easy:
                                        {
                                            run.selectedDifficulty = Deluge.sdd.DifficultyIndex;
                                            Debug.Log("drizzle detected; trying to override");
                                            run.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Difficulty." + Language.GetString(Deluge.sdd.nameToken)));
                                        }
                                        break;
                                    //rainstorm
                                    case DifficultyIndex.Normal:
                                        {
                                            run.selectedDifficulty = Tempest.sdd.DifficultyIndex;
                                            Debug.Log("rainstorm detected; trying to override");
                                            run.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Difficulty." + Language.GetString(Tempest.sdd.nameToken)));
                                        }
                                        break;
                                    //monsoon
                                    case DifficultyIndex.Hard:
                                        {
                                            run.selectedDifficulty = Cyclone.sdd.DifficultyIndex;
                                            Debug.Log("monsoon detected; trying to override");
                                            run.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Difficulty." + Language.GetString(Cyclone.sdd.nameToken)));
                                        }
                                        break;
                                }

                                //typhoon is a modded difficulty and its index is not constant
                                if (ruleChoiceDef.difficultyIndex == Typhoon.sdd.DifficultyIndex)
                                {
                                    run.selectedDifficulty = SuperTyphoon.sdd.DifficultyIndex;
                                    Debug.Log("typhoon detected; trying to override");
                                    run.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Difficulty." + Language.GetString(SuperTyphoon.sdd.nameToken)));
                                    //for some reason appears as deluge in run history???
                                    //appears correctly mid-run & at run end so will ignore for now...
                                }*/

                                diffIndex = run.ruleBook.FindDifficulty();

                                run.RecalculateDifficultyCoefficent();

                                //Debug.Log("hopefully updated diff: " + DifficultyCatalog.GetDifficultyDef(diffIndex).nameToken);

                                //Debug.Log(run.difficultyCoefficient + " - run difficulty coef");
                            }

                            string diffToken = curDiff.nameToken;
                            //Debug.Log(DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue + " - current scaling value");
                            //Debug.Log("ethereals completed: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);
                        }
                        else
                        {
                            teleIsEthereal = false;
                        }
                    }
                }
            }
        }


        // this doesnt change the actual difficulty. i do not want to touch this code holy fuck
        [ConCommand(commandName = "run_set_ethereals_cleared", flags = ConVarFlags.Cheat | ConVarFlags.ExecuteOnServer, helpText = "Sets the number of ethereal teleporters completed. Zero to disable. Format: {etherealsCompleted}")]
        public static void CCSetEtherealsCleared(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;

            int level = args.GetArgInt(0);
            EtherealBehavior etherealBehavior = EtherealBehavior.instance;
            if (etherealBehavior)
            {
                etherealBehavior.etherealsCompleted = level;
            }
        }
    }
}
