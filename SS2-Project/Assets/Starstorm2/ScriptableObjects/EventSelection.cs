using System;
using System.Collections.Generic;
using RoR2.Items;
using UnityEngine;
using RoR2;
using RoR2.ExpansionManagement;
using R2API;
namespace SS2
{
    [CreateAssetMenu(fileName = "EventSelection", menuName = "Starstorm2/EventSelection")]
    public class EventSelection : ScriptableObject
    {
        public DirectorAPI.StageSerde stage;
        public string customStage;
        public bool canStorm;
        //public GameObject stormPrefab; // stormcontroller already handles this and im lazy to port it

        // ideally this is a list of some kind of "EventCategory" struct, with info about the time they spawn, if they can repeat, how many per stage, etc.
        [Header("Elite Event Settings")]
        public Event[] eliteEvents;

        [Header("Misc Event Settings")]
        public Event[] miscEvents;
        [Serializable]
        public struct Event
        {
            public EventCard eventCard;
            public float weight;           
        }

        public static EventSelection GetEventSelectionForStage(Stage stage)
        {
            SceneDef sceneDef = stage.sceneDef;
            foreach(EventSelection eventSelection in instancesList)
            {
                var stageFlags = (DirectorAPI.Stage)eventSelection.stage;
                if (stageFlags.HasFlag(DirectorAPI.GetStageEnumFromSceneDef(sceneDef)))
                {
                    return eventSelection;
                }                   
            }
            return sceneDef.sceneType == SceneType.Stage ? SS2Assets.LoadAsset<EventSelection>("default", SS2Bundle.Events) : null;
        }
        public WeightedSelection<EventCard> GenerateMiscEventWeightedSelection()
        {
            WeightedSelection<EventCard> weightedSelection = new WeightedSelection<EventCard>(miscEvents.Length);
            foreach (Event e in miscEvents)
            {
                if (e.eventCard.IsAvailable())
                {
                    weightedSelection.AddChoice(e.eventCard, e.weight);
                }
            }
            return weightedSelection;
        }

        public WeightedSelection<EventCard> GenerateEliteEventWeightedSelection()
        {
            WeightedSelection<EventCard> weightedSelection = new WeightedSelection<EventCard>(eliteEvents.Length);
            foreach (Event e in eliteEvents)
            {
                if (e.eventCard.IsAvailable())
                {
                    weightedSelection.AddChoice(e.eventCard, e.weight);
                }
            }
            return weightedSelection;
        }

        protected virtual void OnEnable()
        {
            if(Run.instance)
            {
                SS2Log.Info("EventSelection '" + base.name + "' has been loaded after the Run started.");
            }
            EventSelection.instancesList.Add(this);
        }
        protected virtual void OnDisable()
        {
            EventSelection.instancesList.Remove(this);
        }

        private static readonly List<EventSelection> instancesList = new List<EventSelection>();
    }  
}
