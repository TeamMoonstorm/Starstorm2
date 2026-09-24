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
        public static NemesisSpawnCard[] readonlySpawnCards
        {
            get => (NemesisSpawnCard[])allSpawnCards.Clone();
        }
        private static NemesisSpawnCard[] allSpawnCards = Array.Empty<NemesisSpawnCard>();

        internal static NemesisSpawnCard FindSpawnCard(string masterName)
        {
            foreach (NemesisSpawnCard card in allSpawnCards)
            {
                if (card.prefab.name == masterName)
                    return card;
            }
            return null;
        }

        /// <summary>
        /// Adds an AI boss to the invasion pool. Call once on every peer after configuring its prefabs,
        /// normally during content initialization and before starting a run.
        /// The caller must register its content with the game's catalogs; this does not register a survivor or unlock.
        /// Duplicate masters retain their first registration. Late additions affect the next invasion pool selection.
        /// </summary>
        public static void AddNemesis(NemesisCompatInfo info)
        {
            if (!info.masterPrefab)
            {
                SS2Log.Error("NemesisCatalog.AddNemesis: Null masterPrefab. Nemesis invasion was not added.");
                return;
            }
            if (string.IsNullOrEmpty(info.masterPrefab.name))
            {
                SS2Log.Error("NemesisCatalog.AddNemesis: The master prefab must have a catalog-unique name. Nemesis invasion was not added.");
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

            if (!info.masterPrefab.GetComponent<NetworkIdentity>() || !master.bodyPrefab.GetComponent<NetworkIdentity>())
            {
                SS2Log.Error($"NemesisCatalog.AddNemesis: {info.masterPrefab.name} and its body must have NetworkIdentity components. Nemesis invasion was not added.");
                return;
            }

            if (!IsFinite(info.selectionWeight))
            {
                SS2Log.Error($"NemesisCatalog.AddNemesis: {info.masterPrefab.name} has a non-finite selection weight. Nemesis invasion was not added.");
                return;
            }

            if (info.statModifiers != null)
            {
                foreach (var modifier in info.statModifiers)
                {
                    if (!NemesisSpawnCard.TryGetStatField(modifier.fieldName, out _)
                        || !Enum.IsDefined(typeof(NemesisSpawnCard.StatModifierType), modifier.statModifierType)
                        || !IsFinite(modifier.modifier))
                    {
                        SS2Log.Error($"NemesisCatalog.AddNemesis: Invalid stat modifier \"{modifier.fieldName}\" on {info.masterPrefab.name}. Expected a writable float base-stat field, valid operation and finite amount. Nemesis invasion was not added.");
                        return;
                    }
                }
            }

            if (info.skillOverrides != null && info.skillOverrides.Length > 0)
            {
                SkillLocator skillLocator = master.bodyPrefab.GetComponent<SkillLocator>();
                foreach (var skillOverride in info.skillOverrides)
                {
                    if (!skillLocator || !skillLocator.GetSkill(skillOverride.skillSlot) || !skillOverride.skillDef)
                    {
                        SS2Log.Error($"NemesisCatalog.AddNemesis: Invalid {skillOverride.skillSlot} skill override on {info.masterPrefab.name}. Nemesis invasion was not added.");
                        return;
                    }
                }
            }

            Type spawnStateType = info.spawnStateOverride.stateType;
            if (!string.IsNullOrEmpty(info.spawnStateOverride.typeName)
                && (spawnStateType == null || spawnStateType.IsAbstract || spawnStateType.ContainsGenericParameters
                    || !typeof(EntityState).IsAssignableFrom(spawnStateType) || spawnStateType.GetConstructor(Type.EmptyTypes) == null
                    || !EntityStateMachine.FindByCustomName(master.bodyPrefab, "Body")))
            {
                SS2Log.Error($"NemesisCatalog.AddNemesis: Invalid spawn state or missing Body state machine on {info.masterPrefab.name}. Nemesis invasion was not added.");
                return;
            }

            foreach (NemesisSpawnCard card in allSpawnCards)
            {
                if (card.prefab == info.masterPrefab)
                {
                    SS2Log.Warning($"NemesisCatalog.AddNemesis: {info.masterPrefab.name} is already registered. Keeping its first registration.");
                    return;
                }
                if (card.prefab.name == info.masterPrefab.name)
                {
                    SS2Log.Error($"NemesisCatalog.AddNemesis: Another master is already registered as {info.masterPrefab.name}. Master names must be unique for peer setup. Nemesis invasion was not added.");
                    return;
                }
            }

            if (!info.masterPrefab.TryGetComponent(out RoR2.CharacterAI.BaseAI _))
            {
                SS2Log.Warning($"NemesisCatalog.AddNemesis: {info.masterPrefab} does not have a BaseAI component. The boss will spawn but will not act without AI.");
            }

            NemesisSpawnCard spawnCard = ScriptableObject.CreateInstance<NemesisSpawnCard>();
            spawnCard.name = "nsc" + info.masterPrefab.name;
            spawnCard.prefab = info.masterPrefab;
            spawnCard.sendOverNetwork = true;
            spawnCard.hullSize = body.hullClassification;
            spawnCard.nodeGraphType = body.isFlying ? RoR2.Navigation.MapNodeGroup.GraphType.Air : RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            spawnCard.itemDef = info.droppedItem;
            spawnCard.nemesisInventory = info.nemesisInventory;
            spawnCard.visualEffect = info.visualEffect;
            spawnCard.selectionWeight = info.selectionWeight > 0f ? info.selectionWeight : 1f;

            spawnCard.skillOverrides = info.skillOverrides != null ? (NemesisSpawnCard.SkillOverride[])info.skillOverrides.Clone() : Array.Empty<NemesisSpawnCard.SkillOverride>();
            spawnCard.statModifiers = info.statModifiers != null ? (NemesisSpawnCard.StatModifier[])info.statModifiers.Clone() : Array.Empty<NemesisSpawnCard.StatModifier>();
            if (spawnStateType != null)
            {
                spawnCard.overrideSpawnState = info.spawnStateOverride;
                spawnCard.useOverrideState = true;
            }

            HG.ArrayUtils.ArrayAppend(ref allSpawnCards, spawnCard);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        [ConCommand(commandName = "list_nems", flags = ConVarFlags.None, helpText = "Lists all registered nemesis spawn cards with their index.")]
        public static void CCListNemeses(ConCommandArgs args)
        {
            var cards = readonlySpawnCards;
            if (cards == null || cards.Length == 0)
            {
                SS2Log.Info("No nemesis spawn cards registered.");
                return;
            }

            for (int i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                string masterName = card.prefab ? card.prefab.name : "null";
                string itemName = card.itemDef ? Language.GetString(card.itemDef.nameToken) : "none";
                SS2Log.Info($"[{i}] {masterName} (drops: {itemName}, weight: {card.selectionWeight})");
            }
        }

        [ConCommand(commandName = "spawn_nem", flags = ConVarFlags.Cheat | ConVarFlags.ExecuteOnServer, helpText = "Spawns a nemesis boss by catalog index. If no index given, picks randomly. Format: {index}")]
        public static void CCSpawnNemesis(ConCommandArgs args)
        {
            if (!NetworkServer.active) return;
            if (!Run.instance || !DirectorCore.instance || !SceneInfo.instance)
            {
                SS2Log.Error("how tf you callin commands from here");
                return;
            }

            var cards = readonlySpawnCards;
            if (cards == null || cards.Length == 0)
            {
                SS2Log.Error("spawn_nem: No nemesis spawn cards registered.");
                return;
            }

            NemesisSpawnCard spawnCard;
            if (args.Count > 0)
            {
                int index = args.GetArgInt(0);
                if (index < 0 || index >= cards.Length)
                {
                    SS2Log.Error($"spawn_nem: Index {index} out of range. Valid range: 0-{cards.Length - 1}. Use list_nems to see available entries.");
                    return;
                }
                spawnCard = cards[index];
            }
            else
            {
                spawnCard = cards[UnityEngine.Random.Range(0, cards.Length)];
            }

            if (!spawnCard)
            {
                SS2Log.Error("spawn_nem: Selected spawn card is null.");
                return;
            }

            CharacterMaster senderMaster = args.GetSenderMaster();
            if (!senderMaster || !senderMaster.GetBody())
            {
                SS2Log.Error("spawn_nem: No valid body found.");
                return;
            }

            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                spawnOnTarget = senderMaster.GetBody().coreTransform,
                placementMode = DirectorPlacementRule.PlacementMode.NearestNode
            };
            DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Close, out placementRule.minDistance, out placementRule.maxDistance);

            DirectorSpawnRequest request = new DirectorSpawnRequest(spawnCard, placementRule, Run.instance.spawnRng);
            request.teamIndexOverride = TeamIndex.Monster;
            request.ignoreTeamMemberLimit = true;
            request.onSpawnedServer += (result) =>
            {
                if (!result.success || !result.spawnedInstance) return;

                if (result.spawnedInstance.TryGetComponent(out CharacterMaster master))
                {
                    master.inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);
                    int level = Mathf.FloorToInt(Run.instance.ambientLevel);
                    if (level < 60)
                        master.inventory.GiveItem(RoR2Content.Items.LevelBonus, 60 - level);
                }
            };

            if (DirectorCore.instance.TrySpawnObject(request))
                SS2Log.Info($"spawn_nem: Spawned {spawnCard.prefab.name}.");
            else
                SS2Log.Error($"spawn_nem: Could not spawn {spawnCard.prefab.name}.");
        }
    }
}
