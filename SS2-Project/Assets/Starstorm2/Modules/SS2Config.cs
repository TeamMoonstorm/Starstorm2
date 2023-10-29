﻿using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Config;
using Moonstorm.Loaders;
using System.Collections.Generic;
using UnityEngine;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions;

namespace Moonstorm.Starstorm2
{
    public class SS2Config : ConfigLoader<SS2Config>
    {
        public override BaseUnityPlugin MainClass => Starstorm.instance;

        public const string PREFIX = "SS2.";
        internal const string IDMain = PREFIX + "Main";
        internal const string IDItem = PREFIX + "Items";
        internal const string IDArtifact = PREFIX + "Artifacts"; 
        internal const string IDSurvivor = PREFIX + "Survivors";
        internal const string IDMisc = PREFIX + "Miscellaneous";

        public override bool CreateSubFolder => true;

        public static ConfigFile ConfigMain;
        public static ConfigFile ConfigItem;
        public static ConfigFile ConfigArtifact;
        public static ConfigFile ConfigSurvivor;
        public static ConfigFile ConfigMisc;

        internal static ConfigurableBool UnlockAll = new ConfigurableBool(false)
        {
            Section = "General",
            Key = "Unlock All",
            Description = "Setting this to true unlocks all the content in Starstorm 2, excluding skin unlocks.",
            ModGUID = Starstorm.guid,
            ModName = Starstorm.modName,
            CheckBoxConfig = new CheckBoxConfig
            {
                restartRequired = true,
            }
        };

        internal static ConfigEntry<KeyCode> RestKeybind;
        internal static KeyCode restKeybind;
        internal static ConfigEntry<KeyCode> TauntKeybind;
        internal static KeyCode tauntKeybind;
        internal static List<ConfigEntry<bool>> ItemToggles;

        public void Init()
        {
            Sprite icon = SS2Assets.LoadAsset<Sprite>("icon", SS2Bundle.Main);
            ModSettingsManager.SetModIcon(icon, Starstorm.guid, Starstorm.modName);
            ModSettingsManager.SetModDescription("A general content mod adapting ideas from Risk of Rain 1's Starstorm", Starstorm.guid, Starstorm.modName);

            ConfigMain = CreateConfigFile(IDMain, true);
            ConfigItem = CreateConfigFile(IDItem, true);
            ConfigSurvivor = CreateConfigFile(IDSurvivor, true);
            ConfigArtifact = CreateConfigFile(IDArtifact, true);
            ConfigMisc = CreateConfigFile(IDMisc, true);

            SetConfigs();
        }

        private static void SetConfigs()
        {
            UnlockAll.SetConfigFile(ConfigMain).DoConfigure();
            //emotes
            /*RestKeybind = Starstorm.instance.Config.Bind("Starstorm 2 :: Keybinds", "Rest Emote", KeyCode.Alpha1, "Keybind used for the Rest emote.");
            restKeybind = RestKeybind.Value;// cache it for performance

            TauntKeybind = Starstorm.instance.Config.Bind("Starstorm 2 :: Keybinds", "Taunt Emote", KeyCode.Alpha2, "Keybind used for the Taunt emote.");
            tauntKeybind = TauntKeybind.Value;// cache it for performance*/
        }
    }
    /*
    internal static class Config
    {
        #region Config
        //internal static ConfigEntry<bool> ss_test;
        internal static ConfigEntry<bool> UnlockAll;

        internal static ConfigEntry<bool> preRunItemCatalog; //OK
        internal static ConfigEntry<bool> preRunEquipmentCatalog; //OK

        internal static ConfigEntry<KeyCode> RestKeybind;
        internal static KeyCode restKeybind;
        internal static ConfigEntry<KeyCode> TauntKeybind;
        internal static KeyCode tauntKeybind;

        internal static ConfigEntry<bool> EnableItems; //OK
        internal static List<ConfigEntry<bool>> ItemToggles;
        internal static ConfigEntry<bool> EnableEquipment; //OK
        internal static ConfigEntry<bool> EnableFunnyCanister; //OK

        internal static ConfigEntry<bool> TyphoonIncreaseSpawnCap; //OK

        internal static ConfigEntry<bool> EnableEvents; //OK

        internal static ConfigEntry<KeyCode> ExtraKeyCode;

        #endregion

        internal static void Init()
        {
            /*ss_test =
                Starstorm.instance.Config.Bind("Starstorm 2 :: Unfinished Content",
                            "Enabled",
                            false,
                            "Enables Starstorm 2's work-in-progress content. May be unstable so enable at your own risk.");
            preRunItemCatalog =
                Starstorm.instance.Config.Bind("QoL :: Pre-Run Item Catalog",
                    "Enabled",
                    true,
                    "Forces the game to show up the item catalog while in a Lobby for enabling / disabling items.");
            preRunEquipmentCatalog =
                Starstorm.instance.Config.Bind("QoL :: Pre-Run Equipment Catalog",
                    "Enabled",
                    true,
                    "Forces the game to show up the equipment catalog while in a Lobby for enabling / disabling equipment.");
            UnlockAll =
                Starstorm.instance.Config.Bind("Starstorm 2 :: Unlock All",
                            "false",
                            false,
                            "Setting this to true unlocks all the content in Starstorm2, excluding skin unlocks.");
            EnableItems =
                Starstorm.instance.Config.Bind("Starstorm 2 :: Items",
                            "Enabled",
                            true,
                            "Enables Starstorm 2's items. Set to false to disable all of Starstorm 2's items.");
            EnableEquipment =
                Starstorm.instance.Config.Bind("Starstorm 2 :: Equipment",
                            "Enabled",
                            true,
                            "Enables Starstorm 2's equipment. Set to false to disable all of Starstorm 2's equipment.");
            EnableFunnyCanister =
                           Starstorm.instance.Config.Bind("Starstorm 2 :: Equipment",
                                       "Pressurized Canister No Jump Control",
                                       false,
                                       "Set to true to disable jump control on Pressurized Canister - activating the equipment will apply constant upward force regardless of whether you hold the jump button. This may lead to Funny and Memorable (tm) moments, especially if you like picking up Gestures of the Drowned.");

            TyphoonIncreaseSpawnCap =
                Starstorm.instance.Config.Bind("Starstorm 2 :: Typhoon",
                            "Increase Team Limit",
                            true,
                            "Multiplies the Monster Team maximum size by 2 when enabled. Lunar is left unchanged. May affect performance.");
            EnableEvents =
                Starstorm.instance.Config.Bind("Starstorm 2 :: Events",
                            "Enabled",
                            true,
                            "Enables Starstorm 2's random events, including storms. Set to false to disable events.");

            ExtraKeyCode =
                Starstorm.instance.Config.Bind("Starstorm 2 :: Keybinds",
                            "Extra Key",
                            KeyCode.LeftAlt,
                            "Key used for miscelaneous control for characters in Starstorm, such as enabling Borg's heatsinks.");

            //emotes
            /*RestKeybind = Starstorm.instance.Config.Bind("Starstorm 2 :: Keybinds", "Rest Emote", KeyCode.Alpha1, "Keybind used for the Rest emote.");
            restKeybind = RestKeybind.Value;// cache it for performance

            TauntKeybind = Starstorm.instance.Config.Bind("Starstorm 2 :: Keybinds", "Taunt Emote", KeyCode.Alpha2, "Keybind used for the Taunt emote.");
            tauntKeybind = TauntKeybind.Value;// cache it for performance
        }

        // this helper automatically makes config entries for enabling/disabling survivors
        internal static ConfigEntry<bool> CharacterEnableConfig(string characterName)
        {
            return CharacterEnableConfig(characterName, characterName);
        }

        internal static ConfigEntry<bool> CharacterEnableConfig(string characterName, string fullName)
        {
            return Starstorm.instance.Config.Bind(new ConfigDefinition("Starstorm 2 :: " + characterName, "Enabled"), true, new ConfigDescription("Enables Starstorm 2's " + fullName + " survivor. Set to false to disable Starstorm 2's " + fullName + " survivor."));
        }
    }*/
}