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

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
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
            if(item.ItemDef.deprecatedTier != RoR2.ItemTier.NoTier)
            {
                string niceName = MSUtil.NicifyString(item.GetType().Name);
                ConfigEntry<bool> enabled = Starstorm.instance.Config.Bind<bool>(niceName, "Enabled", true, "Should this item be enabled?");

                if (!enabled.Value)
                {
                    item.ItemDef.deprecatedTier = RoR2.ItemTier.NoTier;
                }
            }
        }

    }
}
