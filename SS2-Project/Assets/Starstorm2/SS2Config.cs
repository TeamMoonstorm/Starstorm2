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

        internal static IContentPiece<GameObject>[] bodyObjects;

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
        }

        // should probably try catch. but no thanks
        [SystemInitializer(typeof(ItemTierCatalog))] // need to check if item's tier can drop in game
        public static IEnumerator CreateConfigs()
        {
            EnableItems = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
            {
                b.section = "Enable Items";
                b.key = "Enable All Items";
                b.description = "Enables Starstorm 2's items. Set to false to disable all items";
                b.configFile = SS2Config.ConfigMain;
                b.onConfigChanged += EnableAllItem;
            }).DoConfigure();
            foreach (ItemDef item in SS2Content.SS2ContentPack.itemDefs)
            {
                if(ItemTierCatalog.GetItemTierDef(item.tier)?.isDroppable == true)
                {
                    string niceName = Language.GetString(item.nameToken);
                    var cfg = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
                    {
                        b.section = "Enable Items";
                        b.key = niceName;
                        b.description = $"Should {niceName} appear in game";
                        b.configFile = SS2Config.ConfigMain;
                        b.checkBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableItems.value,
                        };
                    }).DoConfigure();
                    cfg.onConfigChanged += (b) => EnableItem(item, b);
                    EnableItem(item, cfg.value);
                }              
            }
            EnableEquipments = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
            {
                b.section = "Enable Equipments";
                b.key = "Enable All Equipments";
                b.description = "Enables Starstorm 2's equipments. Set to false to disable all equipments";
                b.configFile = SS2Config.ConfigMain;
                b.onConfigChanged += EnableAllEquipment;
            }).DoConfigure();
            foreach (EquipmentDef item in SS2Content.SS2ContentPack.equipmentDefs)
            {
                if (item.canDrop)
                {
                    string niceName = Language.GetString(item.nameToken);
                    var cfg = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
                    {
                        b.section = "Enable Equipments";
                        b.key = niceName;
                        b.description = $"Should {niceName} appear in game";
                        b.configFile = SS2Config.ConfigMain;
                        b.checkBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableEquipments.value,
                        };
                    }).DoConfigure();
                    cfg.onConfigChanged += (b) => EnableItem(item, b);
                    EnableItem(item, cfg.value);
                }
            }
            EnableInteractables = SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
            {
                b.section = "Enable Interactables";
                b.key = "Enable All Interactables";
                b.description = "Enables Starstorm 2's interactables. Set to false to disable interactables.";
                b.configFile = SS2Config.ConfigMain;
                b.onConfigChanged += EnableAllInteractable;
            }).DoConfigure();
            foreach (GameObject interactable in SS2Content.SS2ContentPack.networkedObjectPrefabs)
            {
                if (interactable.TryGetComponent<IInteractable>(out _))
                {
                    ExpansionRequirementComponent item = GetExpansion(interactable);
                    string niceName = MSUtil.NicifyString(interactable.name);
                    var cfg = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
                    {
                        b.section = "Enable Interactables";
                        b.key = niceName;
                        b.description = $"Should {niceName} appear in game";
                        b.configFile = SS2Config.ConfigMain;
                        b.checkBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableInteractables.value,
                        };
                    }).DoConfigure();
                    cfg.onConfigChanged += (b) => EnableItem(item, b);
                    EnableItem(item, cfg.value);
                }
            }
            bodyObjects = ContentUtil.AnalyzeForGameObjectGenericContentPieces<CharacterBody>(SS2Main.Instance).ToArray();
            EnableMonsters = SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
            {
                b.section = "Enable Monsters";
                b.key = "Enable All Monsters";
                b.description = "Enables Starstorm 2's monsters. Set to false to disable monsters.";
                b.configFile = SS2Config.ConfigMain;
                b.onConfigChanged += EnableAllMonster;
            }).DoConfigure();
            EnableSurvivors = SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
            {
                b.section = "Enable Survivors";
                b.key = "Enable All Survivors";
                b.description = "Enables Starstorm 2's survivors. Set to false to disable survivors.";
                b.configFile = SS2Config.ConfigMain;
                b.onConfigChanged += EnableAllSurvivor;
            }).DoConfigure();                     
            foreach (IContentPiece<GameObject> bodyCP in bodyObjects)
            {
                CharacterBody body = null;
                if (bodyCP is SS2Monster monster && (body = monster.CharacterPrefab?.GetComponent<CharacterBody>()))
                {
                    ExpansionRequirementComponent item = GetExpansion(body.gameObject);
                    string niceName = Language.GetString(body.baseNameToken);
                    var cfg = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
                    {
                        b.section = "Enable Monsters";
                        b.key = niceName;
                        b.description = $"Should {niceName} appear in game";
                        b.configFile = SS2Config.ConfigMain;
                        b.checkBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableMonsters.value,
                        };
                    }).DoConfigure();
                    cfg.onConfigChanged += (b) => EnableItem(item, b);
                    EnableItem(item, cfg.value);
                }
                else if (bodyCP is SS2Survivor survivor && (body = survivor.CharacterPrefab?.GetComponent<CharacterBody>()))
                {
                    if (!body) continue;
                    ExpansionRequirementComponent item = GetExpansion(body.gameObject);
                    string niceName = Language.GetString(body.baseNameToken);
                    var cfg = SS2Config.ConfigFactory.MakeConfiguredBool(true, b =>
                    {
                        b.section = "Enable Survivors";
                        b.key = niceName;
                        b.description = $"Should {niceName} appear in game";
                        b.configFile = SS2Config.ConfigMain;
                        b.checkBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableSurvivors.value,
                        };
                    }).DoConfigure();
                    cfg.onConfigChanged += (b) => EnableItem(item, b);
                    EnableItem(item, cfg.value);
                }              
            }
            yield return null;
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

        private static void EnableAllItem(bool enable)
        {
            if(!SS2Assets.assetsAvailability.available)
            {
                // no error message fuck oyu
                return;
            }
            foreach (ItemDef item in SS2Content.SS2ContentPack.itemDefs)
            {
                if (ItemTierCatalog.GetItemTierDef(item.tier)?.isDroppable == true)
                {
                    EnableItem(item, enable);
                }
            }
        }
        private static void EnableAllEquipment(bool enable)
        {
            if (!SS2Assets.assetsAvailability.available)
            {
                return;
            }
            foreach (EquipmentDef item in SS2Content.SS2ContentPack.equipmentDefs)
            {
                if (item.canDrop)
                {
                    EnableItem(item, enable);
                }
            }
        }
        private static void EnableAllSurvivor(bool enable)
        {
            if (!SS2Assets.assetsAvailability.available)
            {
                return;
            }
            foreach (IContentPiece<GameObject> bodyCP in bodyObjects)
            {
                CharacterBody body;
                if (bodyCP is SS2Survivor survivor && (body = survivor.CharacterPrefab?.GetComponent<CharacterBody>()))
                {
                    ExpansionRequirementComponent item = GetExpansion(body.gameObject);
                    EnableItem(item, enable);
                }
            }
        }
        private static void EnableAllMonster(bool enable)
        {
            if (!SS2Assets.assetsAvailability.available)
            {
                return;
            }
            foreach (IContentPiece<GameObject> bodyCP in bodyObjects)
            {
                CharacterBody body;               
                if (bodyCP is SS2Monster monster && (body = monster.CharacterPrefab?.GetComponent<CharacterBody>()))
                {
                    ExpansionRequirementComponent item = GetExpansion(body.gameObject);
                    EnableItem(item, enable);
                }
            }
        }
        private static void EnableAllInteractable(bool enable)
        {
            foreach (GameObject interactable in SS2Content.SS2ContentPack.networkedObjectPrefabs)
            {
                if (interactable.TryGetComponent<IInteractable>(out _))
                {
                    ExpansionRequirementComponent item = GetExpansion(interactable);
                    EnableItem(item, enable);
                }
            }
        }
        private static void EnableItem(ItemDef item, bool enable)
        {
            item.requiredExpansion = enable ? SS2Content.SS2ContentPack.expansionDefs[0] : fuckyou;
        }
        private static void EnableItem(EquipmentDef item, bool enable)
        {
            item.requiredExpansion = enable ? SS2Content.SS2ContentPack.expansionDefs[0] : fuckyou;
        }
        private static void EnableItem(ExpansionRequirementComponent item, bool enable)
        {
            if(item)
                item.requiredExpansion = enable ? SS2Content.SS2ContentPack.expansionDefs[0] : fuckyou;
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