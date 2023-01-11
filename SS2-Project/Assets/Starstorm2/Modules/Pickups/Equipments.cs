using BepInEx.Configuration;
using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Equipments : EquipmentModuleBase
    {
        public static Equipments Instance { get; private set; }
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
                .ToList()
                .ForEach(eqp => AddEquipment(eqp));

            base.GetEquipmentBases().ToList().ForEach(eqp => CheckEnabledStatus(eqp));

            return null;
        }

        protected override IEnumerable<EliteEquipmentBase> GetEliteEquipmentBases()
        {
            base.GetEliteEquipmentBases()
                .ToList()
                .ForEach(eeqp => AddEliteEquipment(eeqp));
            return null;
        }

        protected void CheckEnabledStatus(EquipmentBase eqp)
        {
            string niceName = MSUtil.NicifyString(eqp.GetType().Name);
            if (!(eqp.EquipmentDef.dropOnDeathChance > 0 || eqp.EquipmentDef.passiveBuffDef || niceName.ToLower().Contains("affix")))
            {
                //string niceName = MSUtil.NicifyString(eqp.GetType().Name);
                ConfigEntry<bool> enabled = Starstorm.instance.Config.Bind<bool>(niceName, "Enabled", true, "Should this item be enabled?");
                if (!enabled.Value)
                {
                    eqp.EquipmentDef.appearsInSinglePlayer = false; // :)
                    eqp.EquipmentDef.appearsInMultiPlayer = false;
                    eqp.EquipmentDef.canDrop = false;
                }
            }

            //if (eqp.EquipmentDef.deprecatedTier != RoR2.ItemTier.NoTier)
            //{
            //    string niceName = MSUtil.NicifyString(item.GetType().Name);
            //    ConfigEntry<bool> enabled = Starstorm.instance.Config.Bind<bool>(niceName, "Enabled", true, "Should this item be enabled?");
            //
            //    if (!enabled.Value)
            //    {
            //        item.ItemDef.deprecatedTier = RoR2.ItemTier.NoTier;
            //    }
            //}
        }

    }
}