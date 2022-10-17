using R2API.ScriptableObjects;
using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Items : ItemModuleBase
    {
        public static Items Instance { get; private set; }
        public static ItemDef[] LoadedSS2Items { get => SS2Content.Instance.SerializableContentPack.itemDefs; }
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
                .Where(item => SS2Main.config.Bind<bool>(item.ItemDef.name, "Enable Item", true, "Wether or not to enable this item.").Value)
                .ToList()
                .ForEach(item => AddItem(item));
            return null;
        }
    }
}
