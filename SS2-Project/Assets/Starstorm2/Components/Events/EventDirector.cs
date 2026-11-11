using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2.Components;
using System;

namespace SS2
{
    // fuck the timeline garbage
    // this will behave more like an extra SceneDirector by spawning everything on stage start
    // but with some extra logic to make runs feel smooth, instead of total RNG chaos
    public class EventDirector : NetworkBehaviour
    {
        public static EventDirector instance;

        private void OnEnable()
        {
            SceneDirector.onPrePopulateSceneServer += OnPrePopulateSceneServer;
            Stage.onStageStartGlobal += OnStageStartGlobal;
            instance = this;
        }

        private void OnDisable()
        {
            SceneDirector.onPrePopulateSceneServer -= OnPrePopulateSceneServer;
            Stage.onStageStartGlobal -= OnStageStartGlobal;
            instance = null;
        }

        public EventSelection currentEventSelection;
        private bool finalStage;
        private bool simulacrumRun;

        [NonSerialized]  public float eliteEventChance;
        [NonSerialized] public int stagesUntilInvasion = 5;
        public Xoroshiro128Plus rng { get; private set; }
        private Dictionary<GameObject, int> eventsToMostRecentStage = new Dictionary<GameObject, int>();
        public WeightedSelection<NemesisSpawnCard> availableNemesisSpawnCards { get; private set; }
        private void OnStageStartGlobal(Stage stage)
        {
            // get eventpool for stage
            // pick event timeline
            if (NetworkServer.active)
            {
                finalStage = (stage.sceneDef.stageOrder == 6);
                simulacrumRun = (GameModeCatalog.GetGameModeName(Run.instance.gameModeIndex) == "InfiniteTowerRun");
            }
        }

        private void OnPrePopulateSceneServer(SceneDirector sceneDirector)
        {
            if (NetworkServer.active)
            {
                rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);
                SpawnEvents();
            }
        }

        public void SpawnEvents()
        {
            stagesUntilInvasion--;
            currentEventSelection = EventSelection.GetEventSelectionForStage(SceneCatalog.GetSceneDefForCurrentScene());

            if (currentEventSelection == null || Run.instance.stageClearCount == 0) return;

            float stormTime = 0; // jank ass bandaid for now. push back storm with more events

            // nemesis invasions always appear when available.
            if (TryAddNemesisInvader())
            {
                stormTime += 45f;
            }
            
            if (SS2Config.enableBeta) // FUCK!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            {
                // pick elite event. should be mostly normalized across the run as the rewards are important
                WeightedSelection<EventCard> eliteEvents = currentEventSelection.GenerateEliteEventWeightedSelection();
                if (eliteEvents.Count > 0)
                {
                    eliteEventChance += 20f;
                    if (EtherealBehavior.instance.runIsEthereal)
                        eliteEventChance += 13f;

                    if (Util.CheckRoll(eliteEventChance))
                    {
                        eliteEventChance = 0f;
                        EventCard eliteEvent = eliteEvents.Evaluate(this.rng.nextNormalizedFloat);


                        eliteEvent = SS2Assets.LoadAsset<EventCard>("BlazingElite", SS2Bundle.Events); // temp


                        GameObject eventInstance = GameObject.Instantiate(eliteEvent.eventPrefab);
                        NetworkServer.Spawn(eventInstance);

                        stormTime += 45f;
                    }
                }
            }
            

            // pick mostly random storm start time.
            if (Run.instance.stageClearCount >= 1 && currentEventSelection.canStorm && TeleporterInteraction.instance != null)
            {
                float startTime = UnityEngine.Random.Range(120f, 360f) + stormTime;////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// :(
                GameObject stormController = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("StormController", SS2Bundle.Events));

                var evt = stormController.GetComponent<StormController>();
                evt.stormStartTime = Run.FixedTimeStamp.now + startTime;
                NetworkServer.Spawn(stormController);
            }
        }

        public bool TryAddNemesisInvader()
        {
            // check if any player has voidrock
            // check if its the first stage or every third stage after
            if (SS2Util.GetItemCountForPlayers(SS2Content.Items.VoidRock) > 0 && stagesUntilInvasion <= 0)
            {
                bool anyNemsAvailable = false;
                WeightedSelection<NemesisSpawnCard> selection = new WeightedSelection<NemesisSpawnCard>();
                foreach (NemesisSpawnCard card in NemesisCatalog.readonlySpawnCards)
                {
                    if (SS2Util.GetItemCountForPlayers(card.itemDef) == 0) // no player has the nem's item = this nem can spawn
                    {
                        anyNemsAvailable = true;
                    }
                }

                if (anyNemsAvailable)
                {
                    StartEvent(SS2Assets.LoadAsset<GameObject>("NemesisInvasionEventController", SS2Bundle.Events));
                    stagesUntilInvasion = 3;
                    return true;
                }
            }                             
            
            return false;
        }

        private void StartEvent(GameObject eventPrefab)
        {
            if (!eventPrefab)
            {
                return;
            }
            if (!eventsToMostRecentStage.ContainsKey(eventPrefab))
            {
                eventsToMostRecentStage.Add(eventPrefab, Run.instance.stageClearCount);
            }
            eventsToMostRecentStage[eventPrefab] = Run.instance.stageClearCount;

            //MSU.GameplayEventManager.SpawnGameplayEvent(new MSU.GameplayEventManager.GameplayEventSpawnArgs { gameplayEventPrefab = eventCard.eventPrefab }) // ??????????????????????????????
            GameObject e = GameObject.Instantiate(eventPrefab);
            // fire event here?
            NetworkServer.Spawn(e);
        }
    }

    public class NemesisCatalog
    {
        private static List<NemesisSpawnCard> externalSpawnCards = new List<NemesisSpawnCard>();
        private static List<string> externalMasterNames = new List<string>();
        public static NemesisSpawnCard[] readonlySpawnCards
        {
            get => allSpawnCards;
        }
        private static NemesisSpawnCard[] allSpawnCards;

        [SystemInitializer(typeof(MasterCatalog))]
        private static void Init()
        {
            // collect master names from config
            FromNames();
            allSpawnCards = SS2Assets.LoadAllAssets<NemesisSpawnCard>(SS2Bundle.All);
            HG.ArrayUtils.Join(allSpawnCards, externalSpawnCards.ToArray());
            
        }

        private static void FromNames()
        {
            foreach (string master in externalMasterNames)
            {
                string b1 = !master.EndsWith("Body") ? master : master.Remove(master.Length - 3);
                string m1 = master.EndsWith("Master") ? b1 : b1 + "Master";
                var master1 = MasterCatalog.FindMasterIndex(master);
                if (master1 == MasterCatalog.MasterIndex.none)
                {
                    string m2 = master.EndsWith("MonsterMaster") ? b1 : b1 + "MonsterMaster";
                    master1 = MasterCatalog.FindMasterIndex(m2);
                }
                GameObject masterPrefab = MasterCatalog.GetMasterPrefab(master1);
                if (masterPrefab)
                {
                    AddNemesisInvader(masterPrefab);
                }
                else
                {
                    SS2Log.Error($"NemesisCatalog.FromNames: Could not find master prefab \"{master}\". Nemesis invasion was not added.");
                    return;
                }
            }
        }

        public static void AddNemesisInvader(string masterName)
        {
            externalMasterNames.Add(masterName);
        }
        public static void AddNemesisInvader(GameObject masterPrefab, ItemDef itemDef = null, NemesisSpawnCard.SkillOverride[] skillOverrides = null, EntityStates.SerializableEntityStateType spawnState = default(EntityStates.SerializableEntityStateType))
        {
            if (!masterPrefab)
            {
                SS2Log.Error($"NemesisCatalog.AddNemesisInvader(GameObject): Null master prefab. Nemesis invasion was not added.");
                return;
            }
            if (masterPrefab.TryGetComponent(out CharacterBody _))
            {
                SS2Log.Error($"NemesisCatalog.AddNemesisInvader(GameObject): Expected a CharacterMaster component, but {masterPrefab} is a body prefab. Nemesis invasion was not added.");
                return;
            }
            if (!masterPrefab.TryGetComponent(out CharacterMaster master))
            {
                SS2Log.Error($"NemesisCatalog.AddNemesisInvader(GameObject): Did not find a CharacterMaster component for {masterPrefab}. Nemesis invasion was not added.");
                return;
            }
            if (!master.bodyPrefab || !master.bodyPrefab.TryGetComponent(out CharacterBody body))
            {
                SS2Log.Error($"NemesisCatalog.AddNemesisInvader(GameObject): {masterPrefab} did not have a valid body prefab. Nemesis invasion was not added.");
                return;
            }
            // TODO: base stats
            NemesisSpawnCard spawnCard = ScriptableObject.CreateInstance<NemesisSpawnCard>();
            spawnCard.prefab = masterPrefab;
            spawnCard.hullSize = body.hullClassification;
            spawnCard.nodeGraphType = body.isFlying ? RoR2.Navigation.MapNodeGroup.GraphType.Air : RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            spawnCard.itemDef = itemDef;
            if (skillOverrides != null)
                spawnCard.skillOverrides = skillOverrides;
            if (spawnState.stateType != null)
                spawnCard.overrideSpawnState = spawnState;
            externalSpawnCards.Add(spawnCard);
        }


    }
}
