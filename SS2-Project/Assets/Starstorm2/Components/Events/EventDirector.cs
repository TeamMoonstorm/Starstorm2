using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using SS2.Components;
namespace SS2
{
    public class EventDirector : NetworkBehaviour
    {
    //    public static EventDirector instance;

    //    private void OnEnable()
    //    {
    //        Stage.onStageStartGlobal += OnStageStartGlobal;
    //        EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal += OnNemesisDefeatedGlobal;
    //        instance = this;
    //    }

    //    private void OnNemesisDefeatedGlobal(CharacterBody body)
    //    {
    //        string name = Language.GetString(body.baseNameToken);
    //        Run.instance.SetEventFlag(name + "Defeated");
    //    }

    //    private void OnDisable()
    //    {
    //        Stage.onStageStartGlobal -= OnStageStartGlobal;
    //        instance = null;
    //    }

    //    public EventSelection currentEventSelection;
    //    public EventTimeline currentTimeline;
    //    public float eliteEventChance;
    //    public int stagesUntilInvasion = 5;
    //    private Xoroshiro128Plus rng;
    //    private Dictionary<EventCard, int> eventsToMostRecentStage = new Dictionary<EventCard, int>();
    //    private void OnStageStartGlobal(Stage stage)
    //    {
    //        // get eventpool for stage
    //        // pick event timeline
    //        if (NetworkServer.active)
    //        {
    //            stagesUntilInvasion--;
    //            currentEventSelection = EventSelection.GetEventSelectionForStage(stage);
    //            this.rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
    //            currentTimeline = CreateEventTimeline();
    //        }
            
    //    }

    //    public void PickEventTimeline()
    //    {
    //        // create multiple event timelines, then do random selection weighted by some kind of score given to each timeline
    //        // score increases with event count, overlapping events, and earlier storm times.
    //        // would want to set a "target" score per stage, based on a variety of things. create low and high-event stages
    //    }

    //    // want to create these at the start of each stage rather than randomly spawn them thruout
    //    // ^want to have a visible timeline in the HUD ( behind ruleset/debug/weather radio )
    //    // ^^ might also make it easier to iterate without having to play each time
    //    // going  with almost entirely random events. will hopefully improve later
    //    public EventTimeline CreateEventTimeline()
    //    {
    //        if (currentEventSelection == null || Run.instance.stageClearCount == 0) return null;
    //        EventTimeline eventTimeline = new EventTimeline();
    //        // nemesis invasions always appear when available.
    //        if (TryAddNemesisInvader(ref eventTimeline))
    //        {

    //        }
    //        // pick elite event. should be mostly normalized across the run as the rewards are important
    //        WeightedSelection<EventCard> eliteEvents = currentEventSelection.GenerateEliteEventWeightedSelection();
    //        if (eliteEvents.Count > 0)
    //        {
    //            if(Util.CheckRoll(eliteEventChance))
    //            {
    //                eliteEventChance = 25f;
    //                EventCard eliteEvent = eliteEvents.Evaluate(this.rng.nextNormalizedFloat);
    //                float startTime = UnityEngine.Random.Range(30f, 180f);
    //                eventTimeline.AddEvent(eliteEvent, startTime);
    //            }
    //            else
    //            {
    //                eliteEventChance += 25f;
    //            }
    //        }
    //        // misc events can be thrown in mostly randomly
    //        int miscEventCount = UnityEngine.Random.Range(0, 1 + Run.instance.loopClearCount);      /// ??? lmao   
    //        if(miscEventCount > 0)
    //        {
    //            float startTime = 0f;
    //            WeightedSelection<EventCard> miscEvents = currentEventSelection.GenerateMiscEventWeightedSelection();
    //            for (int i = 0; i < miscEventCount; i++)
    //            {
    //                int index = miscEvents.EvaluateToChoiceIndex(this.rng.nextNormalizedFloat);
    //                EventCard miscEvent = miscEvents.GetChoice(index).value;
    //                miscEvents.RemoveChoice(index);
    //                startTime += UnityEngine.Random.Range(90f, 300f);
    //                eventTimeline.AddEvent(miscEvent, startTime);
    //            }
    //        }
    //        // pick mostly random storm start time.
    //        if (Run.instance.stageClearCount >= 2 && currentEventSelection.canStorm)
    //        {
    //            float startTime = UnityEngine.Random.Range(150f, 420f);
    //            GameObject stormController = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("StormController", SS2Bundle.Events));

    //            var evt = stormController.GetComponent<StormController>();
    //            evt.stormStartTime = Run.FixedTimeStamp.now + startTime;
    //            NetworkServer.Spawn(stormController);
    //            // add to timeline (???)?
    //        }
            
    //        return eventTimeline;
    //    }

    //    private bool IsTeleporterIdle()
    //    {
    //        bool idle = !TeleporterInteraction.instance;
    //        idle |= TeleporterInteraction.instance && TeleporterInteraction.instance.isIdle;
    //        return idle;
    //    }

    //    public bool TryAddNemesisInvader(ref EventTimeline timeline)
    //    {
    //        // check if any player has voidrock
    //        bool voidRock = false;
    //        foreach (var player in PlayerCharacterMasterController.instances)
    //        {
    //            if (player.master.inventory)
    //            {
    //                if (player.master.inventory.GetItemCount(SS2Content.Items.VoidRock) > 0)
    //                {
    //                    voidRock = true;
    //                }
    //            }
    //        }
    //        // check if its the first stage or every third stage after
    //        if(voidRock && stagesUntilInvasion <= 0)
    //        {

    //        }
    //        // get list of possible nemesis invaders
    //        // pick one at random
    //        stagesUntilInvasion = 3;
    //        // return true if found
    //        return false;
    //    }

    //    private void FixedUpdate()
    //    {
    //        // start events when their starttimes have passed
    //        for (int i = 0; i < currentTimeline.events.Count; i++)
    //        {
    //            EventInfo eventInfo = currentTimeline.events[i];
    //            bool teleporter = (eventInfo.eventCard.canStartDuringTeleporterEvent || IsTeleporterIdle());
    //            if (!eventInfo.hasStarted && eventInfo.startTime.hasPassed && teleporter)
    //            {
    //                eventInfo.hasStarted = true;
    //                StartEvent(eventInfo.eventCard);
    //            }
    //        }
    //    }

    //    private void StartEvent(EventCard eventCard)
    //    {
    //        if(!eventsToMostRecentStage.ContainsKey(eventCard))
    //        {
    //            eventsToMostRecentStage.Add(eventCard, Run.instance.stageClearCount);
    //        }
    //        eventsToMostRecentStage[eventCard] = Run.instance.stageClearCount;

    //        GameObject e = GameObject.Instantiate(eventCard.eventPrefab);
    //        // ?
    //        NetworkServer.Spawn(e);
    //    }

    //    public class EventTimeline
    //    {
    //        public List<EventInfo> events;
    //        public float viability;

    //        public bool AddEvent(EventCard eventCard, float startTime)
    //        {
    //            events.Add(new EventInfo(eventCard, startTime));
    //            return true;
    //        }
    //    }

    //    // use this info to display on timeline
    //    public struct EventInfo
    //    {
    //        public EventInfo(EventCard eventCard, float startTime)
    //        {
    //            this.eventCard = eventCard;
    //            this.startTime = Run.FixedTimeStamp.now + startTime;
    //            this.endTime = this.startTime; // + duration
    //            hasStarted = false;
    //        }
    //        public EventCard eventCard;
    //        public Run.FixedTimeStamp startTime;
    //        public Run.FixedTimeStamp endTime;
    //        public bool hasStarted;
    //    }
    //} 
    
    //public class NemesisCatalog
    //{
    //    private static List<NemesisSpawnCard> externalSpawnCards = new List<NemesisSpawnCard>();
    //    private static List<string> externalMasterNames = new List<string>();
    //    [SystemInitializer(typeof(MasterCatalog))]
    //    private static void Init()
    //    {
    //        // collect master names from config
    //        FromNames();
    //    }

    //    private static void FromNames()
    //    {
    //        foreach(string master in externalMasterNames)
    //        {
    //            string b1 = !master.EndsWith("Body") ? master : master.Remove(master.Length - 3);
    //            string m1 = master.EndsWith("Master") ? b1 : b1 + "Master";
    //            var master1 = MasterCatalog.FindMasterIndex(master);
    //            if(master1 == MasterCatalog.MasterIndex.none)
    //            {
    //                string m2 = master.EndsWith("MonsterMaster") ? b1 : b1 + "MonsterMaster";
    //                master1 = MasterCatalog.FindMasterIndex(m2);
    //            }
    //            GameObject masterPrefab = MasterCatalog.GetMasterPrefab(master1);
    //            if(masterPrefab)
    //            {
    //                AddNemesisInvader(masterPrefab);
    //            }
    //            else
    //            {
    //                SS2Log.Error($"NemesisCatalog.FromNames: Could not find master prefab from string \"{master}\". Nemesis invasion was not added.");
    //                return;
    //            }
    //        }
    //    }

    //    public static void AddNemesisInvader(string masterName)
    //    {
    //        externalMasterNames.Add(masterName);
    //    }
    //    public static void AddNemesisInvader(GameObject masterPrefab, ItemDef itemDef = null, NemesisSpawnCard.SkillOverride[] skillOverrides = null, EntityStates.SerializableEntityStateType spawnState = default(EntityStates.SerializableEntityStateType))
    //    {
    //        if(!masterPrefab)
    //        {
    //            SS2Log.Error($"NemesisCatalog.AddNemesisInvader(GameObject): Null master prefab. Nemesis invasion was not added.");
    //            return;
    //        }
    //        if(masterPrefab.TryGetComponent(out CharacterBody _))
    //        {
    //            SS2Log.Error($"NemesisCatalog.AddNemesisInvader(GameObject): Expected a CharacterMaster component, but {masterPrefab} is a body prefab. Nemesis invasion was not added.");
    //            return;
    //        }
    //        if (!masterPrefab.TryGetComponent(out CharacterMaster master))
    //        {
    //            SS2Log.Error($"NemesisCatalog.AddNemesisInvader(GameObject): Did not find a CharacterMaster component for {masterPrefab}. Nemesis invasion was not added.");
    //            return;
    //        }
    //        if(!master.bodyPrefab || !master.bodyPrefab.TryGetComponent(out CharacterBody body))
    //        {
    //            SS2Log.Error($"NemesisCatalog.AddNemesisInvader(GameObject): {masterPrefab} did not have a valid body prefab. Nemesis invasion was not added.");
    //            return;
    //        }
    //        NemesisSpawnCard spawnCard = ScriptableObject.CreateInstance<NemesisSpawnCard>();
    //        spawnCard.prefab = masterPrefab;
    //        spawnCard.hullSize = body.hullClassification;
    //        spawnCard.nodeGraphType = body.isFlying ? RoR2.Navigation.MapNodeGroup.GraphType.Air : RoR2.Navigation.MapNodeGroup.GraphType.Ground;
    //        if(skillOverrides != null)
    //            spawnCard.skillOverrides = skillOverrides;
    //        if(spawnState.stateType != null)
    //            spawnCard.overrideSpawnState = spawnState;
    //        externalSpawnCards.Add(spawnCard);
    //    }

        
    }
}
