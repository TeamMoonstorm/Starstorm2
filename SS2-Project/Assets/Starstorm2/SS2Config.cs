using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions;
using MSU.Config;
using System.Collections;

namespace SS2
{
    public class SS2Config
    {
        public const string PREFIX = "SS2.";
        public const string ID_MAIN = PREFIX + "Main";
        public const string ID_ITEM = PREFIX + "Items";
        public const string ID_ARTIFACT = PREFIX + "Artifacts";
        public const string ID_SURVIVOR = PREFIX + "Survivors";
        public const string ID_MISC = PREFIX + "Miscellaneous";

        internal static ConfigFactory ConfigFactory { get; private set; }

        public static ConfigFile ConfigMain { get; private set; }
        public static ConfigFile ConfigItem { get; private set; }
        public static ConfigFile ConfigArtifact { get; private set; }
        public static ConfigFile ConfigSurvivor { get; private set; }
        public static ConfigFile ConfigMisc { get; private set; }

        internal static ConfiguredBool unlockAll = new ConfiguredBool(false)
        {
            Section = "General",
            Key = "Unlock All",
            Description = "Setting this to true unlocks all the content in Starstorm 2, excluding skin unlocks.",
            ModGUID = SS2Main.GUID,
            ModName = SS2Main.MODNAME,
            CheckBoxConfig = new CheckBoxConfig
            {
                restartRequired = true,
            }
        };

        internal static ConfiguredKeyBind restKeybind;
        internal static ConfiguredKeyBind tauntKeybind;
        internal static List<ConfiguredBool> itemToggles;

        internal static IEnumerator RegisterToModSettingsManager()
        {
            SS2AssetRequest<Sprite> spriteRequest = SS2Assets.LoadAssetAsync<Sprite>("icon", SS2Bundle.Main);
            spriteRequest.StartLoad();

            while (!spriteRequest.IsComplete)
                yield return null;

            ModSettingsManager.SetModIcon(spriteRequest.Asset, SS2Main.GUID, SS2Main.MODNAME);
            ModSettingsManager.SetModDescription("A general content mod adapting ideas from Risk of Rain 1's Starstorm", SS2Main.GUID, SS2Main.MODNAME);
        }

        internal SS2Config(BaseUnityPlugin bup)
        {
            ConfigFactory = new ConfigFactory(bup, true);
            ConfigMain = ConfigFactory.CreateConfigFile(ID_MAIN, true);
            ConfigItem = ConfigFactory.CreateConfigFile(ID_ITEM, true);
            ConfigSurvivor = ConfigFactory.CreateConfigFile(ID_SURVIVOR, true);
            ConfigArtifact = ConfigFactory.CreateConfigFile(ID_ARTIFACT, true);
            ConfigMisc = ConfigFactory.CreateConfigFile(ID_MISC, true);

            unlockAll.WithConfigFile(ConfigMain).DoConfigure();
        }
    }
}