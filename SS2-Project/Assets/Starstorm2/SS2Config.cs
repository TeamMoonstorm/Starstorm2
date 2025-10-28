using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions;
using MSU.Config;
using System.Collections;
using RoR2;
using MSU;
using System;
using RoR2.ExpansionManagement;
using System.Linq;
namespace SS2
{
    public class SS2Config
    {
        public const string PREFIX = "SS2.";
        public const string ID_MAIN = PREFIX + "Main";
        public const string ID_ITEM = PREFIX + "Items";
        public const string ID_ARTIFACT = PREFIX + "Artifacts";
        public const string ID_SURVIVOR = PREFIX + "Survivors";
        public const string ID_MONSTER = PREFIX + "Monsters";
        public const string ID_EVENT = PREFIX + "Events";
        public const string ID_INTERACTABLE = PREFIX + "Interactable";
        public const string ID_MISC = PREFIX + "Miscellaneous";
        public const string ID_ACCESSIBILITY = PREFIX + "Accessibility";

        private static ExpansionDef fuckyou = ScriptableObject.CreateInstance<ExpansionDef>();
        internal static ConfigFactory ConfigFactory { get; private set; }
        public static ConfigFile ConfigMain { get; private set; }
        public static ConfigFile ConfigItem { get; private set; }
        public static ConfigFile ConfigArtifact { get; private set; }
        public static ConfigFile ConfigSurvivor { get; private set; }
        public static ConfigFile ConfigMonster { get; private set; }
        public static ConfigFile ConfigEvent { get; private set; }
        //public static ConfigFile ConfigInteractable { get; private set; }
        public static ConfigFile ConfigMisc { get; private set; }
        public static ConfigFile ConfigAccessibility { get; private set; }

        internal static ConfiguredBool EnableItems;
        public static ConfiguredBool EnableEquipments;
        public static ConfiguredBool EnableInteractables;
        public static ConfiguredBool EnableSurvivors;
        public static ConfiguredBool EnableMonsters;

        [RiskOfOptionsConfigureField(SS2Config.ID_ACCESSIBILITY, configDescOverride = "Intensity of certain flashing effects, from 0%-100%", configNameOverride = "Flashing Effects")]
        public static float FlashingEffectsIntensity = 100f;

        internal static IEnumerator RegisterToModSettingsManager()
        {
            SS2AssetRequest<Sprite> spriteRequest = SS2Assets.LoadAssetAsync<Sprite>("icon", SS2Bundle.Main);
            spriteRequest.StartLoad();

            while (!spriteRequest.IsComplete)
                yield return null;
            Sprite icon = spriteRequest.Asset;
            ModSettingsManager.SetModIcon(icon, SS2Main.GUID, SS2Main.MODNAME);
            ModSettingsManager.SetModIcon(icon, GUID(ID_MAIN), MODNAME(ID_MAIN));
            ModSettingsManager.SetModIcon(icon, GUID(ID_ITEM), MODNAME(ID_ITEM));
            ModSettingsManager.SetModIcon(icon, GUID(ID_SURVIVOR), MODNAME(ID_SURVIVOR));
            ModSettingsManager.SetModIcon(icon, GUID(ID_EVENT), MODNAME(ID_EVENT));
            ModSettingsManager.SetModIcon(icon, GUID(ID_INTERACTABLE), MODNAME(ID_INTERACTABLE));
            ModSettingsManager.SetModIcon(icon, GUID(ID_ARTIFACT), MODNAME(ID_ARTIFACT));
            ModSettingsManager.SetModIcon(icon, GUID(ID_MISC), MODNAME(ID_MISC));
            ModSettingsManager.SetModIcon(icon, GUID(ID_ACCESSIBILITY), MODNAME(ID_ACCESSIBILITY));
            ModSettingsManager.SetModDescription("A general content mod adapting ideas from Risk of Rain 1's Starstorm", SS2Main.GUID, SS2Main.MODNAME);
        }



        internal SS2Config(BaseUnityPlugin bup)
        {

            ConfigFactory = new ConfigFactory(bup, true);          
            ConfigMain = CreateConfigFile(ID_MAIN, true);
            ConfigItem = CreateConfigFile(ID_ITEM, true);           
            ConfigSurvivor = CreateConfigFile(ID_SURVIVOR, true);
            ConfigMonster = CreateConfigFile(ID_MONSTER, true);
            ConfigEvent = CreateConfigFile(ID_EVENT, true);
            //ConfigInteractable = CreateConfigFile(ID_INTERACTABLE, true);
            
            ConfigArtifact = CreateConfigFile(ID_ARTIFACT, true);
            ConfigMisc = CreateConfigFile(ID_MISC, true);
            ConfigAccessibility = CreateConfigFile(ID_ACCESSIBILITY, true);
        }



        private static ExpansionRequirementComponent GetExpansion(GameObject gameObject)
        {
            ExpansionRequirementComponent component = gameObject.GetComponent<ExpansionRequirementComponent>();
            if (!component)
            {
                CharacterMaster component2 = gameObject.GetComponent<CharacterMaster>();
                if (component2 && component2.bodyPrefab)
                {
                    component = component2.bodyPrefab.GetComponent<ExpansionRequirementComponent>();
                }
            }
            return component;
        }
        private static void EnableItem(ItemDef item, bool enable)
        {
            item.requiredExpansion = enable && EnableItems.value ? SS2Content.SS2ContentPack.expansionDefs[0] : fuckyou;
        }
        private static void EnableItem(EquipmentDef item, bool enable)
        {
            item.requiredExpansion = enable && EnableEquipments.value ? SS2Content.SS2ContentPack.expansionDefs[0] : fuckyou;
        }
        private static void EnableInteractable(ExpansionRequirementComponent item, bool enable)
        {
            if(item)
                item.requiredExpansion = enable && EnableInteractables.value ? SS2Content.SS2ContentPack.expansionDefs[0] : fuckyou;
        }
        private static void EnableSurvivor(ExpansionRequirementComponent item, bool enable)
        {
            if (item)
                item.requiredExpansion = enable && EnableSurvivors.value ? SS2Content.SS2ContentPack.expansionDefs[0] : fuckyou;
        }
        private static void EnableMonster(ExpansionRequirementComponent item, bool enable)
        {
            if (item)
                item.requiredExpansion = enable && EnableMonsters.value ? SS2Content.SS2ContentPack.expansionDefs[0] : fuckyou;
        }

        internal static ConfigFile CreateConfigFile(string identifier, bool z = false)
        {          
            return ConfigFactory.CreateConfigFile(identifier, z);
        }

        private static string GUID(string ID)
        {
            return SS2Main.GUID + "." + ID;
        }

        private static string MODNAME(string ID)
        {
            return SS2Main.MODNAME + "." + ID;
        }
    }
}