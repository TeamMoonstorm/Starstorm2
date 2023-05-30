using Moonstorm.Starstorm2.ScriptableObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Moonstorm.Starstorm2
{
    public static class Events
    {

        [ConfigurableField(SS2Config.IDMain, ConfigSection = "Events", ConfigName = ": Enable Events :", ConfigDesc = "Enables Starstorm 2's random events, including storms. Set to false to disable events.")]
        public static bool EnableEvents = true;

        /// <summary>
        /// Class for aiding Nemesis invasion creation for 3rd Parties.
        /// </summary>
        public static class NemesisHelpers
        {
            public static NemesisInventory NemmandoInventory { get => SS2Assets.LoadAsset<NemesisInventory>("NemmandoInventory", SS2Bundle.Events); }
            public static EventCard NemmandoEventCard { get => SS2Assets.LoadAsset<EventCard>("NemmandoBoss", SS2Bundle.Events); }
            public static NemesisSpawnCard NemmandoSpawnCard { get => SS2Assets.LoadAsset<NemesisSpawnCard>("nscNemmandoBoss", SS2Bundle.Events); }

            public static EventCard CreateEventCardFromTemplate(string assetName)
            {
                var copy = Object.Instantiate(NemmandoEventCard);
                copy.name = assetName;
                copy.eventState = default;
                copy.messageColor = Color.white;
                copy.requiredExpansions.Clear();
                return copy;
            }

            public static NemesisSpawnCard CreateNemesisSpawnCardFromTemplate(string assetName)
            {
                var copy = Object.Instantiate(NemmandoSpawnCard);
                copy.name = assetName;
                copy.prefab = null;
                copy.nemesisInventory = null;
                copy.useOverrideState = false;
                copy.overrideSpawnState = default;
                copy.skillOverrides = Array.Empty<NemesisSpawnCard.SkillOverride>();
                copy.visualEffect = null;
                copy.childName = String.Empty;
                copy.itemDef = null;

                return copy;
            }
            /*public class MinimumNemesisData
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
            }*/
        }

        public static void Init()
        {
            if (EnableEvents) foreach (var evt in SS2Assets.LoadAllAssetsOfType<EventCard>(SS2Bundle.Events))
            {
                string name = evt.name;
                if (SS2Config.ConfigMain.Bind("Events", "Enable " + MSUtil.NicifyString(name), true, "Set to false to disable this event.").Value) EventCatalog.AddCard(evt);
            }
            if (SS2Config.ConfigMisc.Bind("Visuals", "Custom Main Menu", true, "Setting this to false returns the main menu to the original, bright one.").Value)
            {
                SceneManager.sceneLoaded += StormOnMenu;
                //On.RoR2.UI.SteamBuildIdLabel.Start += (orig, self) =>
                //{
                //    orig(self);
                //    self.GetComponent<TextMeshProUGUI>().text += "<color=#75BAFF> + <link=\"textWavy\">SS2 " + Starstorm.version.ToString() + "</link></color>";
                //}; // copied from wrb which copied from rm so its my code :smirk_cat:
            }
        }

        private static void StormOnMenu(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.Equals("title"))
            {
                DateTime today = DateTime.Today;
                if ((today.Month == 12) && ((today.Day == 25) || (today.Day == 24)))
                {
                    Object.Instantiate(SS2Assets.LoadAsset<GameObject>("ChristmasMenuEffect", SS2Bundle.Events), Vector3.zero, Quaternion.identity);
                    Debug.Log("Merry Christmas from TeamMoonstorm!! :)");
                }
                else
                {
                    Object.Instantiate(SS2Assets.LoadAsset<GameObject>("StormMainMenuEffect", SS2Bundle.Events), Vector3.zero, Quaternion.identity);
                }
            }   
        }
    }
}
