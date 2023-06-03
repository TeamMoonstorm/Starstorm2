using Moonstorm.Config;
using Moonstorm.Starstorm2.ScriptableObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using RiskOfOptions.OptionConfigs;
using Object = UnityEngine.Object;
using BepInEx.Configuration;
using static System.Collections.Specialized.BitVector32;

namespace Moonstorm.Starstorm2
{
    public static class Events
    {
        public static ConfigurableBool EnableEvents = SS2Config.MakeConfigurableBool(true, b =>
        {
            b.Section = "Events";
            b.Key = "Enable Events";
            b.Description = "Enables Starstorm 2's random events, including storms. Set to false to disable events.";
            b.ConfigFile = SS2Config.ConfigMain;
            b.CheckBoxConfig = new CheckBoxConfig
            {
                restartRequired = true
            };
        }).DoConfigure();

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
        }

        public static void Init()
        {
            if (EnableEvents) foreach (var evt in SS2Assets.LoadAllAssetsOfType<EventCard>(SS2Bundle.Events))
            {
                string name = evt.name;
                var cfg = SS2Config.MakeConfigurableBool(true, b =>
                {
                    b.Section = "Events";
                    b.Key = $"Enable {MSUtil.NicifyString(name)}";
                    b.Description = "Set to false to disable this event";
                    b.ConfigFile = SS2Config.ConfigMain;
                    b.CheckBoxConfig = new CheckBoxConfig
                    {
                        checkIfDisabled = () => !EnableEvents,
                        restartRequired = true,
                    };
                }).DoConfigure();

                if (cfg)
                    EventCatalog.AddCard(evt);
            }

            SS2Config.MakeConfigurableBool(true, b =>
            {
                b.Section = "Visuals";
                b.Key = "Custom Main Menu";
                b.Description = "Setting this to false returns the main menu to the original, bright one.";
                b.ConfigFile = SS2Config.ConfigMisc;
                b.OnConfigChanged += b1 =>
                {
                    if (b1)
                    {
                        SceneManager.sceneLoaded -= StormOnMenu;
                        SceneManager.sceneLoaded += StormOnMenu;
                    }
                    else
                    {
                        SceneManager.sceneLoaded -= StormOnMenu;
                    }
                };
            }).DoConfigure();
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
