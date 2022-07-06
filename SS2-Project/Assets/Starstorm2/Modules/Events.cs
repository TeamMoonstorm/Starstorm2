using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Moonstorm.Starstorm2
{
    public static class Events
    {
        /*
        /// <summary>
        /// Class for Nemesis invasion creation from 3rd Parties.
        /// </summary>
        public static class NemesisHelpers
        {
            public class MinimumNemesisData
            {
                public string identifier;
                public GameObject masterPrefab;
                public SerializableEntityStateType eventState;
                public UnlockableDef requiredUnlockableDef;

                public string childLocatorNameForEffect;
                public GameObject effect;
                public ItemDef itemReward;
                public SerializableEntityStateType overrideSpawnState = new SerializableEntityStateType(typeof(EntityStates.Uninitialized));

                public List<string> scenes = new List<string>();
            }
            public const string genericStartToken = "SS2_EVENT_GENERICNEMESIS_START";
            public const string genericEndToken = "SS2_EVENT_GENERICNEMESIS_END";
            public static Color GenericColor { get => new Color(0.5174214f, 0.1860093f, 0.7169812f, 1); }
            private static EventDirectorCard GenericNemesisEventCard { get => Assets.Instance.MainAssetBundle.LoadAsset<EventDirectorCard>("NemmandoBoss"); }
            private static NemesisSpawnCard GenericSpawnCardData { get => Assets.Instance.MainAssetBundle.LoadAsset<NemesisSpawnCard>("nscNemmando"); }
            private static NemesisInventory GenericNemesisInventory { get => Assets.Instance.MainAssetBundle.LoadAsset<NemesisInventory>("NemmandoInventory"); }

            private static List<(EventDirectorCard, EventSceneDeck[], NemesisSpawnCard)> Nemesis = new List<(EventDirectorCard, EventSceneDeck[], NemesisSpawnCard)>();
            public static (EventDirectorCard, NemesisSpawnCard, EventSceneDeck[]) CreateScriptablesFromData(MinimumNemesisData nemesisData)
            {
                EventDirectorCard edc = ScriptableObject.CreateInstance<EventDirectorCard>();
                edc.activationState = nemesisData.eventState;
                edc.directorCreditCost = GenericNemesisEventCard.directorCreditCost;
                edc.endMessageToken = genericEndToken;
                edc.startMessageToken = genericStartToken;
                edc.eventFlags = GenericNemesisEventCard.eventFlags;
                edc.identifier = nemesisData.identifier;
                edc.messageColor = GenericColor;
                edc.minimumStageCompletions = GenericNemesisEventCard.minimumStageCompletions;
                edc.repeatedSelectionWeight = GenericNemesisEventCard.repeatedSelectionWeight;
                edc.requiredUnlockableDef = nemesisData.requiredUnlockableDef;
                edc.selectionWeight = GenericNemesisEventCard.selectionWeight;

                NemesisSpawnCard nsc = ScriptableObject.CreateInstance<NemesisSpawnCard>();
                nsc.childName = nemesisData.childLocatorNameForEffect;
                nsc.directorCreditCost = GenericSpawnCardData.directorCreditCost;
                nsc.eliteRules = GenericSpawnCardData.eliteRules;
                nsc.forbiddenFlags = GenericSpawnCardData.forbiddenFlags;
                nsc.hullSize = GenericSpawnCardData.hullSize;
                nsc.itemDef = nemesisData.itemReward;
                nsc.nemesisInventory = GenericSpawnCardData.nemesisInventory;
                nsc.nodeGraphType = GenericSpawnCardData.nodeGraphType;
                nsc.occupyPosition = GenericSpawnCardData.occupyPosition;
                nsc.useOverrideState = nemesisData.overrideSpawnState.stateType == typeof(EntityStates.Uninitialized) ? false : true;
                nsc.overrideSpawnState = nsc.useOverrideState ? nemesisData.overrideSpawnState : new SerializableEntityStateType(typeof(Idle));
                nsc.prefab = nemesisData.masterPrefab;
                nsc.requiredFlags = GenericSpawnCardData.requiredFlags;
                nsc.sendOverNetwork = GenericSpawnCardData.sendOverNetwork;
                nsc.skillOverrides = Array.Empty<NemesisSpawnCard.SkillOverride>();
                nsc.statModifiers = GenericSpawnCardData.statModifiers;
                nsc.visualEffect = nemesisData.effect;

                EventSceneDeck[] esds = CreateEventSceneDecks(nemesisData.scenes, edc);

                return (edc, nsc, esds);
            }

            public static EventSceneDeck[] CreateEventSceneDecks(List<string> scenes, EventDirectorCard nemesisEventCard)
            {
                List<EventSceneDeck> esd = new List<EventSceneDeck>();
                foreach(string scene in scenes)
                {
                    EventSceneDeck deck = ScriptableObject.CreateInstance<EventSceneDeck>();
                    deck.sceneName = scene;
                    deck.sceneDeck = new EventCardDeck { eventCards = new EventDirectorCard[1] { nemesisEventCard } };
                    esd.Add(deck);
                }
                return esd.ToArray();
            }

            public static bool AddNemesisInvasion(EventDirectorCard card, EventSceneDeck[] sceneDeck, NemesisSpawnCard spawnCard)
            {
                (EventDirectorCard, EventSceneDeck[], NemesisSpawnCard) tuple = (card, sceneDeck, spawnCard);
                if (!Nemesis.Contains(tuple))
                {
                    Nemesis.Add(tuple);
                    EventCatalog.AddEventDecks(sceneDeck);
                    return true;
                }
                return false;
            }
        }*/

        public static void Init()
        {
            EventCatalog.AddCards(SS2Assets.LoadAllAssetsOfType<EventCard>());
            SceneManager.sceneLoaded += StormOnMenu;
        }

        private static void StormOnMenu(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.Equals("title"))
            {
                if ((DateTime.Today.Month == 12) && ((DateTime.Today.Day == 25) || (DateTime.Today.Day == 24)))
                {
                    Object.Instantiate(SS2Assets.LoadAsset<GameObject>("ChristmasMenuEffect"), Vector3.zero, Quaternion.identity);
                    Debug.Log("Merry Christmas from TeamMoonstorm!! :)");
                }
                else
                {
                    Object.Instantiate(SS2Assets.LoadAsset<GameObject>("StormMainMenuEffect"), Vector3.zero, Quaternion.identity);
                }
            }   
        }
    }
}
