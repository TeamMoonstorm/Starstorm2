using R2API;
using R2API.ScriptableObjects;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
namespace SS2
{
    //if this code looks stupid in any form of the word, please let me know -★
    public class EtherealBehavior : NetworkBehaviour
    {
        public static EtherealBehavior instance { get; private set; }
        public static event Action<EtherealBehavior> onEtherealTeleporterChargedGlobal;
        public static event Action<EtherealBehavior> onDifficultyUpdatedGlobal;
        private static int defaultLevelCap = 99;
        public Run run;
        public static bool alwaysReplaceNewts = true; // dont forget to default to false later
        public static Dictionary<DifficultyIndex, DifficultyIndex> diffDicts = new Dictionary<DifficultyIndex, DifficultyIndex>();
        public static List<EtherealDifficulty> etherealDifficulties = new List<EtherealDifficulty>();
        public static GameObject shrinePrefab;
        public static GameObject portalPrefab;

        [SyncVar]
        public int etherealsCompleted = 0;
        public int etherealStagesCompleted;
        public bool pendingDifficultyUp;
        public bool runIsEthereal;
        public bool runIsEclipse;
        public int bonusLevels = 0;
        private int lastBonusLevels;


        [SystemInitializer]
        internal static void Init()
        {
            //Save default level cap
            defaultLevelCap = Run.ambientLevelCap;

            //Create difficulty indicies dict
            if (SS2Config.enableBeta) // AAAAAAAAAAAAAAAAAAHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            {
                AddEtherealDifficulty(DifficultyIndex.Easy, SS2Assets.LoadAsset<SerializableDifficultyDef>("Deluge", SS2Bundle.Base));
                AddEtherealDifficulty(DifficultyIndex.Normal, SS2Assets.LoadAsset<SerializableDifficultyDef>("Tempest", SS2Bundle.Base));
                AddEtherealDifficulty(DifficultyIndex.Hard, SS2Assets.LoadAsset<SerializableDifficultyDef>("Cyclone", SS2Bundle.Base));
            }
            

            //Initialize related prefabs
            shrinePrefab = SS2Assets.LoadAsset<GameObject>("ShrineEthereal", SS2Bundle.Indev);          
            portalPrefab = SS2Assets.LoadAsset<GameObject>("PortalStranger1", SS2Bundle.SharedStages);
            SS2Content.SS2ContentPack.networkedObjectPrefabs.Add(new GameObject[] { shrinePrefab });
            // On.RoR2.UI.RuleCategoryController.Awake += RuleCategoryController_Awake;
            // On.RoR2.UI.RuleCategoryController.AllocateStrips += RuleCategoryController_AllocateStrips;
            // On.RoR2.UI.RuleCategoryController.AllocateResultIcons += RuleCategoryController_AllocateResultIcons;
            On.RoR2.UI.RuleBookViewerStrip.SetData += RuleBookViewerStrip_SetData;
        }

        private static void RuleBookViewerStrip_SetData(On.RoR2.UI.RuleBookViewerStrip.orig_SetData orig, RuleBookViewerStrip self, List<RuleChoiceDef> newChoices, int choiceIndex)
        {
            orig(self, newChoices, choiceIndex);

            // >>>>>>
            if (self.transform.parent.parent.TryGetComponent(out RuleCategoryController rcc) && rcc.categoryHeaderLanguageController.textMeshPro.text == "Difficulty")
            {
                self.currentDisplayChoiceIndex = 1;
            }

            // stupid fucking way to make sure we're set to Rainstorm by default:
            // if youre seeing this and can do a better job divining the correct way to actually re-set the rule as default on CSS, heres your chance!!
            // i assume its caused because were technically slotting new difficulty options in between drizz and rainstorm, so an index of '1' is technically drizz+1 instead of rainstorm
            // and thats not available on css so it just defaults to 0 or something
            // i really dont know
            // but anyway we just manually force the host to click the button
            // only host actually defaults it- i think is consistent with vanilla
            // by default, only p1 is forced to vote, no one else does -> p1's default vote, rainstorm
            // and if someone else selects a diff, then host will have to change their vote to agree, or otherwise it does whatever happens when its 50/50. idk.
            // ultimately i think it's fine to give host a potential extgra click just to fix this shit already, im so tired of defaulting to drizz
            // also it makes some visual bugs with green squares but id rather have the functionality than whatever tf is happening with that atm
            Transform stupid = self.transform.Find("ChoiceContainer/Choice (Difficulty.Normal)");
            if (NetworkServer.active && stupid != null && stupid.TryGetComponent(out RuleChoiceController omfg))
            {
                omfg.OnClick();
            }
        }

        private static void RuleCategoryController_AllocateResultIcons(On.RoR2.UI.RuleCategoryController.orig_AllocateResultIcons orig, RuleCategoryController self, int desiredCount)
        {
            orig(self, desiredCount);

            // ughhhhhh
            if (self.categoryHeaderLanguageController.textMeshPro.text == "Difficulty")
            {
                SS2Log.Info("found difficulty slider");
                Transform SC = self.transform.Find("StripContainer");
                if (SC != null)
                {
                    Transform RSP = SC.Find("RuleStripPrefab(Clone)");
                    if (RSP != null && RSP.TryGetComponent(out RuleBookViewerStrip rbvs)) // shinji screaming gif
                    {
                        SS2Log.Info("rbvs edit?");
                        SS2Log.Info("rbvs is " + rbvs.currentDisplayChoiceIndex);
                        rbvs.currentDisplayChoiceIndex = 1;
                    }
                }
            }
        }

        private void AmbientLevelDisplay_Update(On.RoR2.UI.AmbientLevelDisplay.orig_Update orig, RoR2.UI.AmbientLevelDisplay self)
        {
            int realLastLevel = self.lastLevel;

            orig(self);

            // add to text if bonus levels > 0 and the level changes
            if (Run.instance != null && Run.instance.ambientLevelFloor != realLastLevel && bonusLevels > 0)
            {
                self.text.text = self.text.text + " <color=#34EB8F>+" + bonusLevels;
            }

            // update to add to text if bonus levels # changes
            if (bonusLevels != lastBonusLevels)
            {
                lastBonusLevels = bonusLevels;
                self.text.text = self.text.text + " <color=#34EB8F>+" + bonusLevels;
            }
        }

        public static void AddEtherealDifficulty(DifficultyIndex baseDifficulty, SerializableDifficultyDef etherealDef)
        {          
            DifficultyAPI.AddDifficulty(etherealDef);
            etherealDifficulties.Add(new EtherealDifficulty(etherealDef));
            diffDicts.Add(baseDifficulty, etherealDef.DifficultyIndex);
        }
        private void Start()
        {
            if (!SS2Config.enableBeta) //  i fucking hate configs ////////////////////////////////////////////////////////////////////////////////////////////////////////
            {
                Destroy(this);
                return;
            }
            run = GetComponentInParent<Run>();
            instance = this;
            runIsEclipse = GameModeCatalog.GetGameModeName(Run.instance.gameModeIndex) == "EclipseRun";
            Run.ambientLevelCap = defaultLevelCap;
            Stage.onServerStageComplete += OnServerStageComplete;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.UI.AmbientLevelDisplay.Update += AmbientLevelDisplay_Update;
        }

        private void OnServerStageComplete(Stage stage)
        {
            if (runIsEthereal && stage.sceneDef.sceneType == SceneType.Stage)
                etherealStagesCompleted++;
        }

        private void OnDestroy()
        {
            Run.ambientLevelCap = defaultLevelCap;
            On.RoR2.SceneDirector.Start -= SceneDirector_Start;
            Stage.onServerStageComplete -= OnServerStageComplete;
            On.RoR2.UI.AmbientLevelDisplay.Update -= AmbientLevelDisplay_Update;
        }

        public void OnEtherealTeleporterCharged()
        {
            pendingDifficultyUp = true;
            onEtherealTeleporterChargedGlobal?.Invoke(this);
        }

        // probably not the right hook but new async stage stuff makes timing a headache so im not going to bother changing it
        public void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);         
            if (self.teleporterInstance && NetworkServer.active)
            {
                SpawnShrine();               
                // from what i could tell, we only wanted to increase difficulty if the new stage had time running
                if (pendingDifficultyUp && SceneCatalog.GetSceneDefForCurrentScene().sceneType == SceneType.Stage)
                    SetEtherealsCompleted(etherealsCompleted+1);
            }
        }

        private void SpawnShrine()
        {
            if (alwaysReplaceNewts)
            {
                ReplaceRandomNewtStatue();
                return;
            }
            var position = Vector3.zero;
            var rotation = Quaternion.Euler(-90, 0, 0);
            string currStage = SceneManager.GetActiveScene().name;
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
                default:
                    ReplaceRandomNewtStatue();
                    return;
                    break;
            }          
            GameObject term = Instantiate(shrinePrefab, position, rotation);
            NetworkServer.Spawn(term);
            
        }

        // pick a random disabled statue to replace with an ethereal shrine
        private void ReplaceRandomNewtStatue()
        {
            PortalStatueBehavior[] statues = GameObject.FindObjectsOfType<PortalStatueBehavior>(true).Where(p => p.portalType == PortalStatueBehavior.PortalType.Shop).ToArray();
            PortalStatueBehavior[] disabledStatues = statues.Where(p => !p.gameObject.activeInHierarchy).ToArray();
            if(disabledStatues.Length > 0) // FUCK newt. that shits getting replaced 25 to 33 percent of the time
            {
                Transform newt = disabledStatues[UnityEngine.Random.Range(0, disabledStatues.Length)].transform;
                GameObject term = Instantiate(shrinePrefab, newt.position + Vector3.up * -1.2f, newt.rotation);
                NetworkServer.Spawn(term);  
            }
            else if (statues.Length > 0)
            {
                Transform newt = statues[UnityEngine.Random.Range(0, statues.Length)].transform;           
                GameObject term = Instantiate(shrinePrefab, newt.position + Vector3.up * -1.2f, newt.rotation);
                NetworkServer.Spawn(term);
                Destroy(newt.gameObject);
            }
            else
            {
                SS2Log.Warning("EtherealBehavior.ReplaceRandomNewtStatue(): No newt statues found for stage!");
                return;
            }
        }

        // if the run isnt on an ethereal difficulty, change it to one. otherwise increase scaling value of ethereal difficulty
        private void SetEtherealsCompleted(int etherealsCompleted)
        {
            this.etherealsCompleted = etherealsCompleted;
            if (etherealsCompleted <= 0)
            {
                //disable ethereals
                // not implementing that because i dont care
                return;
            }
            ChatMessage.Send(Language.GetStringFormatted("SS2_ETHEREAL_DIFFICULTY_WARNING"));
            
            var run = Run.instance;
            DifficultyDef curDiff = DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty);
            
            if (!runIsEthereal && !runIsEclipse)
            {
                runIsEthereal = true;
                run.selectedDifficulty = GetUpdatedDifficulty();
                // run.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Difficulty." + DifficultyCatalog.GetDifficultyDef(run.selectedDifficulty).nameToken));
                for (int i = 0; i < run.ruleBook.ruleValues.Length; ++i)
                {
                    RuleDef rd = RuleCatalog.GetRuleDef(i);
                    RuleChoiceDef rcd = rd.choices[run.ruleBook.ruleValues[i]];
                    if (rcd.difficultyIndex != DifficultyIndex.Invalid)
                    {
                        rcd.difficultyIndex = GetUpdatedDifficulty();
                        // run.ruleBook.ApplyChoice(rcd);
                    }
                }
            }
            else
            {
                // 0.5 per ethereal
                curDiff.scalingValue = EtherealDifficulty.GetDefaultScaling(run.selectedDifficulty) + 0.5f * (etherealsCompleted - 1);
            }

            bonusLevels = 30 * (int)Mathf.Pow(2, etherealsCompleted - 1);

            Run.ambientLevelCap = defaultLevelCap + (100 * etherealsCompleted);  
            run.RecalculateDifficultyCoefficent();
            pendingDifficultyUp = false;          
        }

        //one-time difficulty adjustments - stealing this for shrines too :3 ,,.
        public DifficultyIndex GetUpdatedDifficulty()
        {
            //switch to ethereal difficulty
            DifficultyIndex newDiffIndex;
            DifficultyIndex currentDiffIndex = run.selectedDifficulty;
            
            var diff = DifficultyCatalog.GetDifficultyDef(currentDiffIndex);
            if (!diffDicts.TryGetValue(currentDiffIndex, out newDiffIndex))
            {   
                if(runIsEclipse) return currentDiffIndex;
                
                SS2Log.Warning($"EtherealBehavior.UpdateDifficulty(): Could not find ethereal difficulty for DifficultyDef {Language.GetString(diff.nameToken)}, using it's scaling value instead.");
                if (diff.scalingValue >= 1 && diff.scalingValue < 2 && diffDicts.TryGetValue(DifficultyIndex.Easy, out newDiffIndex)) // if drizzle scaling
                {
                    SS2Log.Warning($"Using Deluge.");
                }
                else if (diff.scalingValue >= 2 && diff.scalingValue < 3 && diffDicts.TryGetValue(DifficultyIndex.Normal, out newDiffIndex)) // if rainstorm scaling
                {
                    SS2Log.Warning($"Using Tempest.");
                }
                else if (diff.scalingValue >= 3 && diff.scalingValue < 3.5f && diffDicts.TryGetValue(DifficultyIndex.Hard, out newDiffIndex)) // if monsoon scaling
                {
                    SS2Log.Warning($"Using Cyclone.");
                }
                else if (diff.scalingValue >= Typhoon.sdd.scalingValue) // if typhoon scaling
                {
                    SS2Log.Warning($"Using SuperTyphoon.");
                    newDiffIndex = Typhoon.sdd.DifficultyIndex;
                }
            }

            return newDiffIndex;
        }

        [ConCommand(commandName = "run_set_ethereals_cleared", flags = ConVarFlags.Cheat | ConVarFlags.ExecuteOnServer, helpText = "Sets the number of ethereal teleporters completed. Zero to disable. Format: {etherealsCompleted}")]
        public static void CCSetEtherealsCleared(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;

            int level = args.GetArgInt(0);
            EtherealBehavior etherealBehavior = EtherealBehavior.instance;
            if (etherealBehavior)
            {
                etherealBehavior.etherealsCompleted = level;
                etherealBehavior.SetEtherealsCompleted(level);
                FindObjectOfType<RoR2.UI.CurrentDifficultyIconController>()?.Start(); // XDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
            }
        }
    }

    public struct EtherealDifficulty
    {
        private static List<EtherealDifficulty> instances = new List<EtherealDifficulty>();
        static EtherealDifficulty()
        {
            Run.onRunDestroyGlobal += ResetAll;
            Run.onRunStartGlobal += ResetAll;
        }
        public static float GetDefaultScaling(DifficultyIndex index)
        {
            foreach (EtherealDifficulty diff in instances)
            {
                if (diff.index == index) return diff.defaultScalingValue;
            }

            return DifficultyCatalog.GetDifficultyDef(index).scalingValue;
        }
        private static void ResetAll(Run obj)
        {
            foreach(EtherealDifficulty diff in instances)
            {
                DifficultyCatalog.GetDifficultyDef(diff.index).scalingValue = diff.defaultScalingValue;
            }         
        }
        public DifficultyIndex index;
        public float defaultScalingValue;
        public EtherealDifficulty(SerializableDifficultyDef def)
        {
            index = def.DifficultyIndex;
            defaultScalingValue = def.scalingValue;
            instances.Add(this);
        }
    }

}