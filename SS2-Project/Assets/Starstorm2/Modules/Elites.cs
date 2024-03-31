using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Moonstorm;
namespace SS2.Modules
{
    public class Elites : EliteModuleBase
    {
        public static Elites Instance { get; private set; }
        public override R2APISerializableContentPack SerializableContentPack { get; } = SS2Content.Instance.SerializableContentPack;
        public override AssetBundle AssetBundle { get; } = SS2Assets.Instance.GetAssetBundle(SS2Bundle.Indev);

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            SS2Log.Info($"Initializing Elites...");
            GetInitializedEliteEquipmentBases();
        }

        protected override IEnumerable<EliteEquipmentBase> GetInitializedEliteEquipmentBases()
        {
            base.GetInitializedEliteEquipmentBases()
                .ToList()
                .ForEach(elite => AddElite(elite));
            return null;
        }
    }
}
