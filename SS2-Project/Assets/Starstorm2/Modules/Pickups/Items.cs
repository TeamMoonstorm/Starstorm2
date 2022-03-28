using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Items : ItemModuleBase
    {
        public static Items Instance { get; private set; }
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
            return null;
        }
    }
}
