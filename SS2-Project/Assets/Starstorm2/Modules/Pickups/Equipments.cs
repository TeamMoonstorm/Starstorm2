using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Equipments : EquipmentModuleBase
    {
        public static Equipments Instance { get; private set; }
        public static EquipmentDef[] LoadedSS2Equipments { get => SS2Content.Instance.SerializableContentPack.equipmentDefs; }
        public override R2APISerializableContentPack SerializableContentPack => SS2Content.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            SS2Log.Info($"Initializing Equipments...");
            GetEquipmentBases();
            GetEliteEquipmentBases();
        }

        protected override IEnumerable<EquipmentBase> GetEquipmentBases()
        {
            base.GetEquipmentBases()
                .Where(eqp => SS2Main.config.Bind<bool>(eqp.EquipmentDef.name, "Enable Equipment", true, "Wether or not to enable this equipment").Value)
                .ToList()
                .ForEach(eqp => AddEquipment(eqp));
            return null;
        }

        protected override IEnumerable<EliteEquipmentBase> GetEliteEquipmentBases()
        {
            base.GetEliteEquipmentBases()
                .ToList()
                .ForEach(eeqp => AddEliteEquipment(eeqp));
            return null;
        }
    }
}
