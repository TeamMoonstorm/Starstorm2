using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
namespace SS2.Modules
{
    public sealed class ItemTiers : ItemTierModuleBase
    {
        public static ItemTiers Instance { get; private set; }
        public override R2APISerializableContentPack SerializableContentPack => SS2Content.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            SS2Log.Info("Initializing Item Tiers...");
            GetItemTierBases();
        }

        protected override IEnumerable<SS2ItemTier> GetItemTierBases()
        {
            base.GetItemTierBases()
                .ToList()
                .ForEach(itemTier => AddItemTier(itemTier));
            return null;
        }
    }
}