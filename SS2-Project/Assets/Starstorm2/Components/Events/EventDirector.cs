using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2.Components;
using EntityStates;

namespace SS2
{
    public class EventDirector : NetworkBehaviour
    {
        public static EventDirector instance;

        private void OnEnable()
        {
            Stage.onStageStartGlobal += OnStageStartGlobal;
            instance = this;
        }

        private void OnDisable()
        {
            Stage.onStageStartGlobal -= OnStageStartGlobal;
            instance = null;
        }

        public EventSelection currentEventSelection;
        private bool finalStage;
        private bool simulacrumRun;
        public EventTimeline currentTimeline;
        public float eliteEventChance;
        public int stagesUntilInvasion = 5;
        public Xoroshiro128Plus rng { get; private set; }
        private Dictionary<GameObject, int> eventsToMostRecentStage = new Dictionary<GameObject, int>();
        public WeightedSelection<NemesisSpawnCard> availableNemesisSpawnCards { get; private set; }
        private void OnStageStartGlobal(Stage stage)
        {
            // get eventpool for stage
            // pick event timeline
            if (NetworkServer.active)
            {
                stagesUntilInvasion--;
                currentEventSelection = EventSelection.GetEventSelectionForStage(stage);
                finalStage = (stage.sceneDef.stageOrder == 6);
                simulacrumRun = (GameModeCatalog.GetGameModeName(Run.instance.gameModeIndex) == "InfiniteTowerRun");
                this.rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
                currentTimeline = CreateEventTimeline();
            }

        }

        public void PickEventTimeline()
        {
            // create multiple event timelines, then do random selection weighted by some kind of score given to each timeline
            // score increases with event count, overlapping events, and earlier storm times.
            // would want to set a "target" score per stage, based on a variety of things. create low and high-event stages
        }

        // want to create these at the start of each stage rather than randomly spawn them thruout
        // ^want to have a visible timeline in the HUD ( behind ruleset/debug/weather radio )
        // ^^ might also make it easier to iterate without having to play each time
        // going  with almost entirely random events for now. will hopefully improve later
        public EventTimeline CreateEventTimeline()
        {
            
            EventTimeline eventTimeline = new EventTimeline();
            if (currentEventSelection == null || Run.instance.stageClearCount == 0) return eventTimeline;
            float stormTime = 0; // jank ass bandaid for now. push back storm with more events
            // nemesis invasions always appear when available.
            if (TryAddNemesisInvader(ref eventTimeline))
            {
                stormTime += 45f;
            }
            
            bool useFinalStageEliteEvents = (!finalStage || finalStage && Events.EnableMoon2EliteEvents);
            //bool useSimulacrumEliteEvents = (!simulacrumRun || simulacrumRun && Events.EnableSimulacrumEliteEvents);
            if (SS2Config.enableBeta && useFinalStageEliteEvents) // FUCK!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            {
                // pick elite event. should be mostly normalized across the run as the rewards are important
                WeightedSelection<EventCard> eliteEvents = currentEventSelection.GenerateEliteEventWeightedSelection();
                if (EtherealBehavior.instance.runIsEthereal)
                    eliteEventChance += 17.5f;
                if (eliteEvents.Count > 0)
                {
                    if (Util.CheckRoll(eliteEventChance))
                    {
                        eliteEventChance = 25f;
                        EventCard eliteEvent = eliteEvents.Evaluate(this.rng.nextNormalizedFloat);
                        float startTime = UnityEngine.Random.Range(30f, 120f);//////////////////////////////////////////////////////////////////////////////////////////////////////
                        eventTimeline.AddEvent(eliteEvent.eventPrefab, startTime);
                        stormTime += 45f;
                    }
                    else
                    {
                        eliteEventChance += 25f;
                    }
                }
                // misc events can be thrown in mostly randomly
                int miscEventCount = UnityEngine.Random.Range(0, 1 + Run.instance.loopClearCount);      /// ??? lmao   
                if (miscEventCount > 0)
                {
                    float startTime = 0f;
                    WeightedSelection<EventCard> miscEvents = currentEventSelection.GenerateMiscEventWeightedSelection();
                    if (miscEvents.Count > 0)
                    {
                        for (int i = 0; i < miscEventCount; i++)
                        {
                            int index = miscEvents.EvaluateToChoiceIndex(this.rng.nextNormalizedFloat);
                            EventCard miscEvent = miscEvents.GetChoice(index).value;
                            miscEvents.RemoveChoice(index);
                            startTime += UnityEngine.Random.Range(90f, 300f); /////////////////////////////////////////////////////////////////////////////////////////////
                            eventTimeline.AddEvent(miscEvent.eventPrefab, startTime);
                        }
                    }
                }
            }
            

            bool useFinalStageStorms = (!finalStage || finalStage && Events.EnableMoon2Storms);
            bool useSimulacrumStorms = (!simulacrumRun || simulacrumRun && Events.EnableSimulacrumStorms);
            // pick mostly random storm start time.
            if (Run.instance.stageClearCount >= 1 && currentEventSelection.canStorm && useFinalStageStorms && useSimulacrumStorms)
            {
                float startTime = UnityEngine.Random.Range(120f, 360f) + stormTime;////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// :(
                GameObject stormController = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("StormController", SS2Bundle.Events));

                var evt = stormController.GetComponent<StormController>();
                evt.stormStartTime = Run.FixedTimeStamp.now + startTime;
                NetworkServer.Spawn(stormController);
                // add to timeline (???)?
            }

            return eventTimeline;
        }

        private bool IsTeleporterIdle()
        {
            bool idle = !TeleporterInteraction.instance;
            idle |= TeleporterInteraction.instance && TeleporterInteraction.instance.isIdle;
            return idle;
        }

        public bool TryAddNemesisInvader(ref EventTimeline timeline)
        {
            // check if any player has voidrock
            // check if its the first stage or every third stage after
            if (SS2Util.GetItemCountForPlayers(SS2Content.Items.VoidRock) > 0 && stagesUntilInvasion <= 0)
            {
                // get list of possible nemesis invaders
                // doing it by drop makes more sense imo? dont want two stirring souls with ss2u.
                // also cool thematically. like they keep reviving if you dont claim the item
                WeightedSelection<NemesisSpawnCard> selection = new WeightedSelection<NemesisSpawnCard>();
                foreach(NemesisSpawnCard card in NemesisCatalog.readonlySpawnCards)
                {                    
                    if(card.itemDef == null || SS2Util.GetItemCountForPlayers(card.itemDef) == 0)
                        selection.AddChoice(card, card.selectionWeight > 0f ? card.selectionWeight : 1f);
                }
                // pick one at random
                availableNemesisSpawnCards = selection;
                if (selection.Count == 0) return false;
                NemesisSpawnCard nemesisSpawnCard = selection.Evaluate(rng.nextNormalizedFloat);
                if(nemesisSpawnCard)
                {
                    timeline.AddEvent(SS2Assets.LoadAsset<GameObject>("NemesisInvasionEventController", SS2Bundle.Events), 3);
                    stagesUntilInvasion = 3;
                    return true;
                }
                else
                {
                    SS2Log.Error("null NemesisSpawnCard");
                }
            }                             
            
            return false;
        }

        private void FixedUpdate()
        {
            if (currentTimeline == null || currentTimeline.events == null) // TODO: FIGURE OUT WTF THIS MEANS !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            {
                return;
            }
            // start events when their starttimes have passed
            if (!NetworkServer.active || !Stage.instance || currentTimeline.events.Count == 0) return; // lol wtf
            for (int i = 0; i < currentTimeline.events.Count; i++)
            {
                EventInfo eventInfo = currentTimeline.events[i];
                bool teleporter = (eventInfo.canStartDuringTeleporterEvent || IsTeleporterIdle());
                if (!eventInfo.hasStarted && eventInfo.startTime.hasPassed && teleporter)
                {
                    eventInfo.hasStarted = true;
                    StartEvent(eventInfo.eventPrefab);
                }
                if(eventInfo.hasStarted && eventInfo.endTime.hasPassed)
                {
                    EndEvent(); /////////////////////////////////////////////////////////////////////
                }
            }
        }

        // ?
        private void StartEvent(GameObject eventPrefab)
        {
            if(!eventPrefab)
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

        private void EndEvent()
        {
            // destroy it?
            // tell the object to end itself?
        }

        public class EventTimeline
        {
            public List<EventInfo> events = new List<EventInfo>();
            public float viability;

            public bool AddEvent(GameObject eventPrefab, float startTime, float endTime = Mathf.Infinity, bool canStartDuringTeleporterEvent = false)
            {
                events.Add(new EventInfo(eventPrefab, startTime, endTime, canStartDuringTeleporterEvent));
                return true;
            }
        }

        // use this info to display on timeline
        public class EventInfo
        {
            public EventInfo(GameObject eventPrefab, float startTime, float endTime, bool canStartDuringTeleporterEvent)
            {
                this.eventPrefab = eventPrefab;
                this.startTime = Run.FixedTimeStamp.now + startTime;
                this.endTime = this.startTime + endTime;
                this.canStartDuringTeleporterEvent = canStartDuringTeleporterEvent;
                hasStarted = false;
            }
            public GameObject eventPrefab;
            public Run.FixedTimeStamp startTime;
            public Run.FixedTimeStamp endTime;
            public bool hasStarted;
            public bool canStartDuringTeleporterEvent;
        }
    }

    public class NemesisCatalog
    {
        private static List<NemesisSpawnCard> externalSpawnCards = new List<NemesisSpawnCard>();
        private static List<string> externalMasterNames = new List<string>();
        private static bool hasInitialized;

        public static NemesisSpawnCard[] readonlySpawnCards
        {
            get => allSpawnCards;
        }
        private static NemesisSpawnCard[] allSpawnCards;

        [SystemInitializer(typeof(MasterCatalog))]
        private static void Init()
        {
            FromNames();
            allSpawnCards = SS2Assets.LoadAllAssets<NemesisSpawnCard>(SS2Bundle.All);
            allSpawnCards = HG.ArrayUtils.Join(allSpawnCards, externalSpawnCards.ToArray());
            hasInitialized = true;
        }

        private static void FromNames()
        {
            foreach (string master in externalMasterNames)
            {
                string baseName = master.EndsWith("Body") ? master.Remove(master.Length - 4) : master;
                string masterName = baseName.EndsWith("Master") ? baseName : baseName + "Master";
                var masterIndex = MasterCatalog.FindMasterIndex(masterName);
                if (masterIndex == MasterCatalog.MasterIndex.none)
                {
                    string monsterMasterName = baseName + "MonsterMaster";
                    masterIndex = MasterCatalog.FindMasterIndex(monsterMasterName);
                }
                GameObject masterPrefab = MasterCatalog.GetMasterPrefab(masterIndex);
                if (masterPrefab)
                {
#pragma warning disable CS0618 // TODO: We're using the obsolete method here hence the pragma, but lets change this
                    AddNemesisInvader(masterPrefab);
#pragma warning restore CS0618
                }
                else
                {
                    SS2Log.Error($"NemesisCatalog.FromNames: Could not find master prefab \"{master}\". Nemesis invasion was not added.");
                    continue;
                }
            }
        }

        /// <summary>
        /// Register a nemesis boss using a NemesisCompatInfo struct.
        /// Call before MasterCatalog initialization for guaranteed inclusion.
        /// Can also be called after initialization (spawn card is added immediately).
        /// </summary>
        public static void AddNemesis(NemesisCompatInfo info)
        {
            if (!info.masterPrefab)
            {
                SS2Log.Error("NemesisCatalog.AddNemesis: Null masterPrefab. Nemesis invasion was not added.");
                return;
            }
            if (info.masterPrefab.TryGetComponent(out CharacterBody _))
            {
                SS2Log.Error($"NemesisCatalog.AddNemesis: Expected a CharacterMaster component, but {info.masterPrefab} is a body prefab. Nemesis invasion was not added.");
                return;
            }
            if (!info.masterPrefab.TryGetComponent(out CharacterMaster master))
            {
                SS2Log.Error($"NemesisCatalog.AddNemesis: Did not find a CharacterMaster component for {info.masterPrefab}. Nemesis invasion was not added.");
                return;
            }
            if (!master.bodyPrefab || !master.bodyPrefab.TryGetComponent(out CharacterBody body))
            {
                SS2Log.Error($"NemesisCatalog.AddNemesis: {info.masterPrefab} did not have a valid body prefab. Nemesis invasion was not added.");
                return;
            }

            if (!info.masterPrefab.TryGetComponent(out RoR2.CharacterAI.BaseAI _))
            {
                SS2Log.Warning($"NemesisCatalog.AddNemesis: {info.masterPrefab} does not have a BaseAI component. The boss will spawn but will not act without AI.");
            }

            NemesisSpawnCard spawnCard = ScriptableObject.CreateInstance<NemesisSpawnCard>();
            spawnCard.prefab = info.masterPrefab;
            spawnCard.hullSize = body.hullClassification;
            spawnCard.nodeGraphType = body.isFlying ? RoR2.Navigation.MapNodeGroup.GraphType.Air : RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            spawnCard.itemDef = info.droppedItem;
            spawnCard.selectionWeight = info.selectionWeight;

            if (info.skillOverrides != null)
                spawnCard.skillOverrides = info.skillOverrides;
            if (info.statModifiers != null)
                spawnCard.statModifiers = info.statModifiers;
            if (info.spawnStateOverride.stateType != null)
            {
                spawnCard.overrideSpawnState = info.spawnStateOverride;
                spawnCard.useOverrideState = true;
            }

            if (hasInitialized)
            {
                HG.ArrayUtils.ArrayAppend(ref allSpawnCards, spawnCard);
                SS2Log.Info($"NemesisCatalog.AddNemesis: Registered {info.masterPrefab.name} after catalog initialization.");
            }
            else
            {
                externalSpawnCards.Add(spawnCard);
            }
        }

        [Obsolete("Use AddNemesis(NemesisCompatInfo) instead.")]
        public static void AddNemesisInvader(string masterName)
        {
            externalMasterNames.Add(masterName);
        }

        [Obsolete("Use AddNemesis(NemesisCompatInfo) instead.")]
        public static void AddNemesisInvader(GameObject masterPrefab, ItemDef itemDef = null, NemesisSpawnCard.SkillOverride[] skillOverrides = null, SerializableEntityStateType spawnState = default(SerializableEntityStateType))
        {
            AddNemesis(new NemesisCompatInfo
            {
                masterPrefab = masterPrefab,
                droppedItem = itemDef,
                skillOverrides = skillOverrides,
                spawnStateOverride = spawnState,
            });
        }
    }
}
