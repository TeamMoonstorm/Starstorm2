using R2API;
using R2API.ScriptableObjects;
using R2API.Utils;
using RoR2;
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
        private static int defaultLevelCap;
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
        internal static IEnumerator Init()
        {
            //Save default level cap
            defaultLevelCap = Run.ambientLevelCap;

            //Create difficulty indicies dict
            AddEtherealDifficulty(DifficultyIndex.Easy, SS2Assets.LoadAsset<SerializableDifficultyDef>("Deluge", SS2Bundle.Base));
            AddEtherealDifficulty(DifficultyIndex.Normal, SS2Assets.LoadAsset<SerializableDifficultyDef>("Tempest", SS2Bundle.Base));
            AddEtherealDifficulty(DifficultyIndex.Hard, SS2Assets.LoadAsset<SerializableDifficultyDef>("Cyclone", SS2Bundle.Base));

            //Initialize related prefabs
            shrinePrefab = SS2Assets.LoadAsset<GameObject>("ShrineEthereal", SS2Bundle.Indev);          
            portalPrefab = SS2Assets.LoadAsset<GameObject>("PortalStranger1", SS2Bundle.SharedStages);
            SS2Content.SS2ContentPack.networkedObjectPrefabs.Add(new GameObject[] { shrinePrefab, portalPrefab });         
            yield return null;
        }

        public static void AddEtherealDifficulty(DifficultyIndex baseDifficulty, SerializableDifficultyDef etherealDef)
        {          
            DifficultyAPI.AddDifficulty(etherealDef);
            etherealDifficulties.Add(new EtherealDifficulty(etherealDef));
            diffDicts.Add(baseDifficulty, etherealDef.DifficultyIndex);
        }
        private void Start()
        {
            run = GetComponentInParent<Run>();
            instance = this;
            Run.ambientLevelCap = defaultLevelCap;
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
        }

        private void OnDestroy()
        {
            Run.ambientLevelCap = defaultLevelCap;
            On.RoR2.SceneDirector.Start -= SceneDirector_Start;
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
            if (runIsEthereal)
                etherealStagesCompleted++;
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
            if(false)//disabledStatues.Length > 0) // FUCK newt. that shits getting replaced 25 to 33 percent of the time
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
            
            if (!runIsEthereal)
            {
                runIsEthereal = true;
                UpdateDifficulty();
            }
            else
            {
                // 0.5 per ethereal
                curDiff.scalingValue = EtherealDifficulty.GetDefaultScaling(run.selectedDifficulty) + 0.5f * (etherealsCompleted - 1);
            }

            Run.ambientLevelCap = defaultLevelCap + (100 * etherealsCompleted);          
            run.RecalculateDifficultyCoefficent();
            pendingDifficultyUp = false;          
        }

        //one-time difficulty adjustments
        public void UpdateDifficulty()
        {
            //switch to ethereal difficulty
            DifficultyIndex newDiffIndex;
            DifficultyIndex currentDiffIndex = run.selectedDifficulty;
            var diff = DifficultyCatalog.GetDifficultyDef(currentDiffIndex);
            if (!diffDicts.TryGetValue(currentDiffIndex, out newDiffIndex))
            {            
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
            run.selectedDifficulty = newDiffIndex;
            run.ruleBook.ApplyChoice(RuleCatalog.FindChoiceDef("Difficulty." + DifficultyCatalog.GetDifficultyDef(newDiffIndex).nameToken));
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
                if (diff.index == index) return diff.defaultScalingValue;
            return 0f;
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