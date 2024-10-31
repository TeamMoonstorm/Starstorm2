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

        private static ExpansionDef fuckyou = ScriptableObject.CreateInstance<ExpansionDef>();
        internal static ConfigFactory ConfigFactory { get; private set; }
        public static ConfigFile ConfigMain { get; private set; }
        public static ConfigFile ConfigItem { get; private set; }
        public static ConfigFile ConfigArtifact { get; private set; }
        public static ConfigFile ConfigSurvivor { get; private set; }
        //public static ConfigFile ConfigMonster { get; private set; }
        public static ConfigFile ConfigEvent { get; private set; }
        //public static ConfigFile ConfigInteractable { get; private set; }
        public static ConfigFile ConfigMisc { get; private set; }

        internal static ConfiguredBool unlockAll = new ConfiguredBool(false)
        {
            section = "General",
            key = "Unlock All",
            description = "Setting this to true unlocks all the content in Starstorm 2, excluding skin unlocks.",
            modGUID = SS2Main.GUID,
            modName = SS2Main.MODNAME,
        };
        internal static ConfiguredBool EnableItems;
        public static ConfiguredBool EnableEquipments;
        public static ConfiguredBool EnableInteractables;
        public static ConfiguredBool EnableSurvivors;
        public static ConfiguredBool EnableMonsters;

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
            ModSettingsManager.SetModDescription("A general content mod adapting ideas from Risk of Rain 1's Starstorm", SS2Main.GUID, SS2Main.MODNAME);
        }



        internal SS2Config(BaseUnityPlugin bup)
        {

            ConfigFactory = new ConfigFactory(bup, true);          
            ConfigMain = CreateConfigFile(ID_MAIN, true);
            ConfigItem = CreateConfigFile(ID_ITEM, true);           
            ConfigSurvivor = CreateConfigFile(ID_SURVIVOR, true);
            //ConfigMonster = CreateConfigFile(ID_MONSTER, true);
            ConfigEvent = CreateConfigFile(ID_EVENT, true);
            //ConfigInteractable = CreateConfigFile(ID_INTERACTABLE, true);
            
            ConfigArtifact = CreateConfigFile(ID_ARTIFACT, true);
            ConfigMisc = CreateConfigFile(ID_MISC, true);
            unlockAll.WithConfigFile(ConfigMain).DoConfigure();

            RoR2Application.onLoad += CreateConfigs;
        }

        // should probably try catch. but no thanks
        // tried to make this work at runtime at first. gave up. its good enough for now
        public static void CreateConfigs()
        {
            EnableItems = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
            {
                b.section = "Enable Items";
                b.key = "Enable All Items";
                b.description = "Enables Starstorm 2's items. Set to false to disable all items";
                b.configFile = SS2Config.ConfigMain;
                b.checkBoxConfig = new CheckBoxConfig
                {
                    restartRequired = true,
                };
                //b.onConfigChanged += EnableAllItem;
            }).DoConfigure();            
            foreach (ItemDef item in SS2Content.SS2ContentPack.itemDefs)
            {
                if(ItemTierCatalog.GetItemTierDef(item.tier)?.isDroppable == true)
                {
                    string niceName = Language.GetString(item.nameToken).Replace("'", String.Empty);
                    var cfg = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
                    {
                        b.section = "Enable Items";
                        b.key = niceName;
                        b.description = $"Should {niceName} appear in game";
                        b.configFile = SS2Config.ConfigMain;
                        b.checkBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableItems.value,
                            restartRequired = true,
                        };
                    }).DoConfigure();
                    //cfg.onConfigChanged += (b) => EnableItem(item, b);
                    EnableItem(item, cfg.value);
                }              
            }
            EnableEquipments = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
            {
                b.section = "Enable Equipments";
                b.key = "Enable All Equipments";
                b.description = "Enables Starstorm 2's equipments. Set to false to disable all equipments";
                b.configFile = SS2Config.ConfigMain;
                b.checkBoxConfig = new CheckBoxConfig
                {
                    restartRequired = true,
                };
                //b.onConfigChanged += EnableAllEquipment;
            }).DoConfigure();
            foreach (EquipmentDef item in SS2Content.SS2ContentPack.equipmentDefs)
            {
                if (item.canDrop)
                {
                    string niceName = Language.GetString(item.nameToken).Replace("'", String.Empty);
                    var cfg = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
                    {
                        b.section = "Enable Equipments";
                        b.key = niceName;
                        b.description = $"Should {niceName} appear in game";
                        b.configFile = SS2Config.ConfigMain;
                        b.checkBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableEquipments.value,
                            restartRequired = true,
                        };
                    }).DoConfigure();
                    //cfg.onConfigChanged += (b) => EnableItem(item, b);
                    EnableItem(item, cfg.value);
                }
            }
            EnableInteractables = SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
            {
                b.section = "Enable Interactables";
                b.key = "Enable All Interactables";
                b.description = "Enables Starstorm 2's interactables. Set to false to disable interactables.";
                b.configFile = SS2Config.ConfigMain;
                //b.onConfigChanged += EnableAllInteractable;
            }).DoConfigure();
            foreach (InteractableSpawnCard isc in SS2Assets.LoadAllAssets<InteractableSpawnCard>(SS2Bundle.All))
            {
                GameObject interactable = isc.prefab;
                if (interactable?.TryGetComponent<IInteractable>(out _) == true)
                {
                    ExpansionRequirementComponent item = GetExpansion(interactable);
                    if (!item) continue;
                    string niceName = MSUtil.NicifyString(interactable.name);
                    if(interactable.TryGetComponent<GenericDisplayNameProvider>(out var name))
                    {
                        niceName = Language.GetString(name.displayToken);
                    }
                    else if (interactable.TryGetComponent<PurchaseInteraction>(out var purchaseInteraction))
                    {
                        niceName = purchaseInteraction.displayNameToken;
                    }
                    niceName = niceName.Replace("'", String.Empty);
                    var cfg = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
                    {
                        b.section = "Enable Interactables";
                        b.key = niceName;
                        b.description = $"Should {niceName} appear in game";
                        b.configFile = SS2Config.ConfigMain;
                        b.checkBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableInteractables.value,
                            restartRequired = true,
                        };
                    }).DoConfigure();
                    //cfg.onConfigChanged += (b) => EnableItem(item, b);
                    EnableInteractable(item, cfg.value);
                }
            }
            EnableMonsters = SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
            {
                b.section = "Enable Monsters";
                b.key = "Enable All Monsters";
                b.description = "Enables Starstorm 2's monsters. Set to false to disable monsters.";
                b.configFile = SS2Config.ConfigMain;
                //b.onConfigChanged += EnableAllMonster;
            }).DoConfigure();            
            EnableSurvivors = SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
            {
                b.section = "Enable Survivors";
                b.key = "Enable All Survivors";
                b.description = "Enables Starstorm 2's survivors. Set to false to disable survivors.";
                b.configFile = SS2Config.ConfigMain;
                b.checkBoxConfig = new CheckBoxConfig
                {
                    restartRequired = true,
                };
                //b.onConfigChanged += EnableAllSurvivor;
            }).DoConfigure();         
            List<GameObject> uniquePrefabs = new List<GameObject>();
            foreach(CharacterSpawnCard csc in SS2Assets.LoadAllAssets<CharacterSpawnCard>(SS2Bundle.All))
            {
                if(!(csc is NemesisSpawnCard) && csc.prefab && !uniquePrefabs.Contains(csc.prefab))
                {
                    uniquePrefabs.Add(csc.prefab);
                    CharacterMaster master = csc.prefab.GetComponent<CharacterMaster>();
                    if (!master || (master && !master.bodyPrefab)) continue;
                    if (!SS2Content.SS2ContentPack.bodyPrefabs.Contains(master.bodyPrefab)) continue;
                    ExpansionRequirementComponent item = GetExpansion(master.gameObject);
                    if (!item) continue;
                    string niceName = Language.GetString(master.bodyPrefab.GetComponent<CharacterBody>().baseNameToken).Replace("'", String.Empty);
                    var cfg = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
                    {
                        b.section = "Enable Monsters";
                        b.key = niceName;
                        b.description = $"Should {niceName} appear in game";
                        b.configFile = SS2Config.ConfigMain;
                        b.checkBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableMonsters.value,
                            restartRequired = true,
                        };
                    }).DoConfigure();
                    //cfg.onConfigChanged += (b) => EnableItem(item, b);
                    EnableMonster(item, cfg.value);
                }
            }
            foreach (SurvivorDef sd in SS2Content.SS2ContentPack.survivorDefs)
            {
                if (sd.bodyPrefab)
                {                   
                    CharacterBody body = sd.bodyPrefab.GetComponent<CharacterBody>();
                    ExpansionRequirementComponent item = body.GetComponent<ExpansionRequirementComponent>();
                    if (!item) continue;
                    string niceName = Language.GetString(body.baseNameToken).Replace("'", String.Empty);
                    var cfg = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
                    {
                        b.section = "Enable Survivors";
                        b.key = niceName;
                        b.description = $"Should {niceName} appear in game";
                        b.configFile = SS2Config.ConfigMain;
                        b.checkBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableSurvivors.value,
                            restartRequired = true,
                        };
                    }).DoConfigure();
                    //cfg.onConfigChanged += (b) => EnableItem(item, b);
                    EnableSurvivor(item, cfg.value);
                }
            }
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