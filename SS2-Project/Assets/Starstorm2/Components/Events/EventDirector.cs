//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;
//using RoR2;

//namespace SS2
//{
//    public class EventDirector : NetworkBehaviour
//    {
//        public static EventDirector instance;

//        [SystemInitializer]
//        private static void Init()
//        {
//            Run.onRunStartGlobal += OnRunStartGlobal;
//            Stage.onStageStartGlobal += OnStageStartGlobal;
//        }

//        private static void OnStageStartGlobal(Stage stage)
//        {
//            // get eventpool for stage
//            // pick event timeline
//        }

//        private static void OnRunStartGlobal(Run run)
//        {
//            // clear stuff
//            // spawn director
//        }


//        public EventTimeline currentTimeline;
//        public void PickEventTimeline()
//        {
//            // create multiple event timelines, then do random selection weighted by viability
//        }

//        public EventTimeline CreateEventTimeline(int credit)
//        {
//            EventTimeline eventTimeline = new EventTimeline();
//            // no events stage 1
//            if (Run.instance.stageClearCount == 0)
//            {
//                credit -= 100;
//            }
//            // less credit on stage 4 (AWU, gold chests)
//            if (Run.instance.stageClearCount == 3)
//            {
//                credit -= 30;
//            }
//            // less credit if there is a void seed
//            if (TeamComponent.GetTeamMembers(TeamIndex.Void).Count > 0)
//            {
//                credit -= 30;
//            }
//            // nemesis invasions always appear when available. low credit cost as they should be disruptive
//            if (TryAddNemesisInvader(ref eventTimeline))
//            {
//                credit -= 10;
//            }          
//            // pick elite event. should be mostly normalized across the run as the rewards are important
//            // misc events can be thrown in mostly randomly with low credit cost
//            // pick mostly random storm start time. faster storms and starting during other events costs credit
//            return null;
//        }

//        public bool TryAddNemesisInvader(ref EventTimeline timeline)
//        {
//            // check if any player has voidrock
//            // check if its the first stage or every third stage after
//            // get list of possible nemesis invaders
//            // pick one at random
//            // return true if found
//            return false;
//        }

//        private void FixedUpdate()
//        {
//            // start events when their starttimes have passed
//            for (int i = 0; i < currentTimeline.events.Length; i++)
//            {
//                EventInfo eventInfo = currentTimeline.events[i];
//                if(!eventInfo.hasStarted && eventInfo.startTime.hasPassed)
//                {
//                    eventInfo.hasStarted = true;
//                    // start event
//                }
//            }
//        }

//        public class EventTimeline
//        {
//            public EventInfo[] events;
//            public float viability; 
//        }

//        public struct EventInfo
//        {
//            public GameObject prefab;
//            public Run.FixedTimeStamp startTime;
//            public bool hasStarted;
//        }
//    }


//    [CreateAssetMenu(fileName = "EventPool", menuName = "Starstorm2/EventPool")]
//    public class EventPool : ScriptableObject
//    {
//        public R2API.DirectorAPI.StageSerde stage;

//        public GameObject stormEventPrefab;
//        public Event[] eliteEvents;
//        public Event[] miscEvents;

//        private readonly WeightedSelection<CurseIndex> eliteSelector = new WeightedSelection<CurseIndex>();
//        private readonly WeightedSelection<CurseIndex> miscSelector = new WeightedSelection<CurseIndex>();
//        protected static CurseIndex[] GenerateUniqueCursesFromWeightedSelection(int maxDrops, Xoroshiro128Plus rng, WeightedSelection<CurseIndex> weightedSelection)
//        {
//            int num = Math.Min(maxDrops, weightedSelection.Count);
//            int[] array = Array.Empty<int>();
//            CurseIndex[] array2 = new CurseIndex[num];
//            for (int i = 0; i < num; i++)
//            {
//                int choiceIndex = weightedSelection.EvaluateToChoiceIndex(rng.nextNormalizedFloat, array);
//                WeightedSelection<CurseIndex>.ChoiceInfo choice = weightedSelection.GetChoice(choiceIndex);
//                array2[i] = choice.value;
//                Array.Resize<int>(ref array, i + 1);
//                array[i] = choiceIndex;
//            }
//            return array2;
//        }

//        protected static CurseIndex GenerateCurseFromWeightedSelection(Xoroshiro128Plus rng, WeightedSelection<CurseIndex> weightedSelection)
//        {
//            if (weightedSelection.Count > 0)
//            {
//                int choiceIndex = weightedSelection.EvaluateToChoiceIndex(rng.nextNormalizedFloat);
//                CurseIndex curseIndex = weightedSelection.choices[choiceIndex].value;
//                return curseIndex;
//            }
//            return CurseIndex.None;
//        }

//        public CurseIndex GenerateCurse(Xoroshiro128Plus rng)
//        {
//            CurseIndex pickupIndex = GenerateCurseFromWeightedSelection(rng, this.eliteSelector);
//            if (pickupIndex == CurseIndex.None)
//            {
//                SS2Log.Error("Could not generate curse index from CursePool.");
//            }
//            return pickupIndex;
//        }

//        public CurseIndex[] GenerateUniqueCurses(int maxDrops, Xoroshiro128Plus rng)
//        {
//            CurseIndex[] array = GenerateUniqueCursesFromWeightedSelection(maxDrops, rng, this.eliteSelector);
//            return array;
//        }

//        public bool IsEmpty()
//        {
//            return this.eliteSelector.Count == 0;
//        }

//        protected void Regenerate(Run run)
//        {
//            this.eliteSelector.Clear();
//            foreach (Event curse in this.eliteEvents)
//            {
//                for (int i = 0; i < curse.count; i++)
//                    this.eliteSelector.AddChoice(curse.curseIndex, curse.weight);

//                if (!allCurses.Contains(curse)) allCurses.Add(curse); // sry
//            }
//        }

//        protected virtual void OnEnable()
//        {
//            EventPool.instancesList.Add(this);
//            if (Run.instance)
//            {
//                SS2Log.Info("CursePool '" + base.name + "' has been loaded after the Run started.  This might be an issue with asset duplication across bundles, or it might be fine.  Regenerating...");
//                this.Regenerate(Run.instance);
//            }
//        }
//        protected virtual void OnDisable()
//        {
//            EventPool.instancesList.Remove(this);
//        }

//        static EventPool()
//        {
//            EventPool.onCursesRefreshed += EventPool.RegenerateAll;
//        }

//        private static void RegenerateAll(Run run)
//        {
//            for (int i = 0; i < EventPool.instancesList.Count; i++)
//            {
//                EventPool.instancesList[i].Regenerate(run);
//            }
//        }

//        private static readonly List<EventPool> instancesList = new List<EventPool>();
//    }

//    public class Event
//    {
//        public GameObject prefab;
//        public float weight;
//    }

//    public enum EventType
//    {
//        Storm,
//        Elite,
//        Misc,
//    }
//}
