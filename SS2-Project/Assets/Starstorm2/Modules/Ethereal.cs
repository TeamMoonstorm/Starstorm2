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
    //LASCIATE OGNI SPERANZA, VOI CH'ENTRATE
    class Ethereal
    {
        public static Ethereal instance { get; private set; }
        public static Color32 ethColor = new Color32(18, 93, 74, 255);

        public static int etherealsCompleted { get; private set; }
        public static bool teleIsEthereal;
        private static float storedScalingValue;

        public static bool teleUpgraded;
        internal static void Init()
        {
            //Initialize Scavenger trading
            Components.TraderController.Initialize();

            //Hooks
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
            TeleporterInteraction.onTeleporterBeginChargingGlobal += TeleporterInteraction_onTeleporterBeginChargingGlobal;
            TeleporterInteraction.onTeleporterFinishGlobal += TeleporterInteraction_onTeleporterFinishGlobal;
            On.RoR2.TeleporterInteraction.FixedUpdate += TeleporterInteraction_FixedUpdate;
            On.RoR2.TeleporterInteraction.Start += TeleporterInteraction_Start;

            //DISABLING ALL OF THIS CONTENT IS AS SIMPLE AS JUST DISABLING THIS HOOK!!!
            //obv won't prevent content loading - that should go without saying, but will make it all inaccessible by player via standard gameplay
            //On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }

        private static void Run_onRunStartGlobal(Run run)
        {
            //Debug.Log("Setting up Ethereals..");
            etherealsCompleted = 0;
            teleIsEthereal = false;
            Debug.Log("completed: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);
        }

        private static void Run_onRunDestroyGlobal(Run run)
        {
            DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue = storedScalingValue;
            Debug.Log(DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue + " - scaling value");
            Debug.Log("completed ethereals: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);
        }

        private static void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);

            if (self.teleporterInstance)
            {
                var position = Vector3.zero;
                var rotation = Quaternion.Euler(-90, 0, 0);
                var currStage = SceneManager.GetActiveScene().name;

                //add the ethereal terminal to a unique position per stage
                //i hope you aren't here to cheat...
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
                    case "skymeadow":
                        position = new Vector3(65.9f, 127.4f, -293.9f);
                        rotation = Quaternion.Euler(0, 194.8f, 0);
                        break;
                        //on top of the tallest rock spire, opposite side of map from the moon
                    case "snowyforest":
                        position = new Vector3(-38.7f, 112.7f, 153.1f);
                        rotation = Quaternion.Euler(0, 54.1f, 0);
                        break;
                        //on top of a lone elevated platform on a tree
                    case "ancientloft":
                        position = new Vector3(-133.4f, 33.5f, -280f);
                        rotation = Quaternion.Euler(0, 354.5f, 0);
                        break;
                        //on a branch under the main platform in the back corner of the map
                    case "sulfurpools":
                        position = new Vector3(-33.6f, 36.8f, 164.1f);
                        rotation = Quaternion.Euler(0, 187f, 0);
                        //in the corner, atop of one of the columns
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

                    //update difficulty
                    if (NetworkServer.active)
                    {
                        run.RecalculateDifficultyCoefficent();

                        DifficultyIndex diffIndex = run.ruleBook.FindDifficulty();

                        Debug.Log("og difficulty diff: " + DifficultyCatalog.GetDifficultyDef(diffIndex).nameToken);

                        for (int i = 0; i < run.ruleBook.ruleValues.Length; i++)
                        {
                            RuleChoiceDef ruleChoiceDef = run.ruleBook.GetRuleChoice(i);

                            switch(ruleChoiceDef.difficultyIndex)
                            {
                                //drizzle
                                case DifficultyIndex.Easy:
                                    {
                                        run.selectedDifficulty = Deluge.DelugeIndex;
                                        Debug.Log("drizzle detected; trying to override");
                                        run.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Difficulty." + Language.GetString(Deluge.DelugeDef.nameToken)));
                                    }
                                    break;
                                //rainstorm
                                case DifficultyIndex.Normal:
                                    {
                                        run.selectedDifficulty = Tempest.TempestIndex;
                                        Debug.Log("rainstorm detected; trying to override");
                                        run.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Difficulty." + Language.GetString(Tempest.TempestDef.nameToken)));
                                    }
                                    break;
                                //monsoon
                                case DifficultyIndex.Hard:
                                    {
                                        run.selectedDifficulty = Cyclone.CycloneIndex;
                                        Debug.Log("monsoon detected; trying to override");
                                        run.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Difficulty." + Language.GetString(Cyclone.CycloneDef.nameToken)));
                                    }
                                    break;
                            }

                            //typhoon is a modded difficulty and its index is not constant
                            if (ruleChoiceDef.difficultyIndex == Typhoon.TyphoonIndex)
                            {
                                run.selectedDifficulty = SuperTyphoon.SuperTyphoonIndex;
                                Debug.Log("typhoon detected; trying to override");
                                run.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Difficulty." + Language.GetString(SuperTyphoon.SuperTyphoonDef.nameToken)));
                                //for some reason appears as deluge in run history???
                                //appears correctly mid-run & at run end so will ignore for now...
                            }
                        }

                        diffIndex = run.ruleBook.FindDifficulty();

                        run.RecalculateDifficultyCoefficent();

                        Debug.Log("hopefully updated diff: " + DifficultyCatalog.GetDifficultyDef(diffIndex).nameToken);

                        Debug.Log(run.difficultyCoefficient + " - run difficulty coef");
                    }
        
                    string diffToken = curDiff.nameToken;
                    Debug.Log(DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).scalingValue + " - current scaling value");
                    Debug.Log("ethereals completed: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);
                }
            }
        }

        private static void TeleporterInteraction_Start(On.RoR2.TeleporterInteraction.orig_Start orig, TeleporterInteraction self)
        {
            orig(self);

            //on start: grab the teleporter model, and set the fresnel back to the regular red
            Debug.Log("setting tp back");
            var newTeleMat = SS2Assets.LoadAsset<Material>("matEtherealFresnelOverlayOG", SS2Bundle.Indev);
            var teleBase = self.gameObject.transform.Find("TeleporterBaseMesh").gameObject;
            var teleProngs = teleBase.transform.Find("TeleporterProngMesh").gameObject;
            var teleBeacon = teleBase.transform.Find("SurfaceHeight").Find("TeleporterBeacon").gameObject;
            teleBase.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
            teleProngs.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
            teleBeacon.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
        }

        private static void TeleporterInteraction_FixedUpdate(On.RoR2.TeleporterInteraction.orig_FixedUpdate orig, TeleporterInteraction self)
        {
            orig(self);

            if (teleIsEthereal && !teleUpgraded)
            {
                Debug.Log("tele changing...");
                //flag the teleporter as modified
                teleUpgraded = true;

                //set a bunch of variables we'll be using for teleporter modifications:
                var newTeleMat = SS2Assets.LoadAsset<Material>("matEtherealFresnelOverlay", SS2Bundle.Indev);
                var teleBase = self.gameObject.transform.Find("TeleporterBaseMesh").gameObject;
                var teleProngs = teleBase.transform.Find("TeleporterProngMesh").gameObject;
                var teleBeacon = teleBase.transform.Find("SurfaceHeight").Find("TeleporterBeacon").gameObject;
                var teleParticleSphere = teleBase.transform.Find("BuiltInEffects").Find("PassiveParticle, Sphere").gameObject;
                var teleParticleCenter = teleBase.transform.Find("BuiltInEffects").Find("PassiveParticle, Center").gameObject;

                //update the fresnel material from red to green
                teleBase.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                teleProngs.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                teleBeacon.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);

                //resize & reposition the teleporter
                teleBase.transform.localScale *= 1.5f;
                teleBase.transform.position = new Vector3(teleBase.transform.position.x, teleBase.transform.position.y + 2.5f, teleBase.transform.position.z);
                //teleProngs.transform.localScale *= 2f;
                teleBeacon.transform.localScale *= 0.5f;

                //resize & recolor large teleporter particles
                ParticleSystem telePassiveParticles = teleParticleSphere.GetComponent<ParticleSystem>();
                teleParticleSphere.transform.localScale *= 2f;
                telePassiveParticles.startColor = new Color(.8f, .32f, .39f);

                //resize & recolor particles above teleporter well
                ParticleSystem teleCenterParticles = teleParticleCenter.GetComponent<ParticleSystem>();
                teleCenterParticles.startColor = new Color(.8f, .32f, .39f);

                //there's so many vfx to replace and recolor.
                //it'd maybe be easier to make a new prefab entirely although i can't help but feel it makes more sense to just edit the existing...
                //maybe particle system prefabs in unity & code to adjust scale / pos of the objects?
                //hell either way.
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

                    Debug.Log("ethereals completed: " + etherealsCompleted + "; teleIsEthereal: " + teleIsEthereal);

                    if (tele.bossDirector)
                    {
                        tele.bossDirector.monsterCredit += (float)(int)(300f * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f));
                        Debug.Log("added to monstercred");
                    }
                    if (tele.bonusDirector)
                    {
                        tele.bonusDirector.monsterCredit += (float)(int)(300f * Mathf.Pow(Run.instance.compensatedDifficultyCoefficient, 0.5f));
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
        {
            //nothing yet lol
        }
    }
}


