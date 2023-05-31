using BepInEx;
using BepInEx.Configuration;
using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Items : ItemModuleBase
    {
        public static Items Instance { get; private set; }

        public BaseUnityPlugin MainClass => Starstorm.instance;
        public override R2APISerializableContentPack SerializableContentPack => SS2Content.Instance.SerializableContentPack;

        //[ConfigurableField(SS2Config.IDItem, ConfigSection = ": Enable All Items :", ConfigName = ": Enable All Items :", ConfigDesc = "Enables Starstorm 2's items. Set to false to disable items.")]
        public static ConfigEntry<bool> EnableItem;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            EnableItem = SS2Config.ConfigItem.Bind(": Enable All Items :", ": Enable All Items :", true, "Enables Starstorm 2's items. Set to false to disable all items.");
            SS2Log.Info($"Initializing Items...");
            GetItemBases();
        }

        protected override IEnumerable<ItemBase> GetItemBases()
        {
            base.GetItemBases()
                .ToList()
                .ForEach(item => AddItem(item));

            base.GetItemBases().ToList().ForEach(item => CheckEnabledStatus(item));

            return null;
        }

        protected void CheckEnabledStatus(ItemBase item)
        {
            if (item.ItemDef.deprecatedTier != RoR2.ItemTier.NoTier || item.ItemDef.tier == RoR2.ItemTier.AssignedAtRuntime) //fix for sybl
            {
                string niceName = MSUtil.NicifyString(item.GetType().Name);
                ConfigEntry<bool> enabled = SS2Config.ConfigItem.Bind(niceName, "Enabled", true, "Should this item be enabled?");
                //SS2Log.Info("EnabledItems checking " + item.ItemDef.nameToken );
                if (!EnableItem.Value || !enabled.Value)
                {
                    //SS2Log.Info("Disabling " + niceName);
                    item.ItemDef.deprecatedTier = RoR2.ItemTier.NoTier;
                }
            }
        }

    }
}
