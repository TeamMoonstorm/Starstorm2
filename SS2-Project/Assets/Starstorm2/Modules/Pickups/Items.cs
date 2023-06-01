using BepInEx;
using BepInEx.Configuration;
using Moonstorm.Config;
using R2API.ScriptableObjects;
using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Items : ItemModuleBase
    {
        public static Items Instance { get; private set; }

        public BaseUnityPlugin MainClass => Starstorm.instance;
        public override R2APISerializableContentPack SerializableContentPack => SS2Content.Instance.SerializableContentPack;

        public static ConfigurableBool EnableItems = new ConfigurableBool(true)
        {
            Section = "Enable All Items",
            Key = "Enable All Items",
            Description = "Enables Starstorm 2's items. Set to false to disable all items",
        }
        //[ConfigurableField(SS2Config.IDItem, ConfigSection = ": Enable All Items :", ConfigName = ": Enable All Items :", ConfigDesc = "Enables Starstorm 2's items. Set to false to disable items.")]
        public static ConfigEntry<bool> EnableItems;

        private static IEnumerable<ItemBase> items;
        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            EnableItems = SS2Config.ConfigItem.Bind(": Enable All Items :", ": Enable All Items :", true, "Enables Starstorm 2's items. Set to false to disable all items.");
            SS2Log.Info($"Initializing Items...");
            items = GetItemBases();
        }

        [SystemInitializer(typeof(ItemTierCatalog))]  // done for custom tiered items that doesn't work with deprecatedtier. -P
        public static void TierInit()
        {
            SS2Log.Info($"Post-Initializing Items...");
            items.ToList().ForEach(i => { if (i.ItemDef.deprecatedTier == ItemTier.NoTier) i.ItemDef.tier = ItemTier.NoTier; });
        }

        protected override IEnumerable<ItemBase> GetItemBases()
        {
            List<ItemBase> list = base.GetItemBases().ToList();
            list.ForEach(item => AddItem(item));
            list.ForEach(CheckEnabledStatus);
            return list;
        }

        public void CheckEnabledStatus(ItemBase item)
        {
            if (item.ItemDef.deprecatedTier != ItemTier.NoTier || item.ItemDef.tier == ItemTier.AssignedAtRuntime) //fix for sybl
            {
                string niceName = MSUtil.NicifyString(item.GetType().Name);
                ConfigEntry<bool> enabled = SS2Config.ConfigItem.Bind(niceName, "Enabled", true, "Should this item be enabled?");
                //SS2Log.Info("EnabledItems checking " + item.ItemDef.nameToken );
                if (!EnableItems.Value || !enabled.Value)
                {
                    //SS2Log.Info("Disabling " + niceName);
                    item.ItemDef.deprecatedTier = ItemTier.NoTier;
                }
            }
        }

    }
}
