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

        //[ConfigurableField(SS2Config.IDItem, ConfigSection = ": Enable All Equipments :", ConfigName = ": Enable All Equipments :", ConfigDesc = "Enables Starstorm 2's equipments. Set to false to disable equipments.")]
        public static ConfigEntry<bool> EnableEquipments;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            EnableEquipments = SS2Config.ConfigItem.Bind(": Enable All Equipments :", ": Enable All Equipments :", true, "Enables Starstorm 2's equipments. Set to false to disable all equipments.");
            SS2Log.Info($"Initializing Equipments...");
            GetEquipmentBases();
            GetEliteEquipmentBases();
        }

        protected override IEnumerable<EquipmentBase> GetEquipmentBases()
        {
            base.GetEquipmentBases()
                .ToList()
                .ForEach(eqp => {
                    if (eqp.EquipmentDef.cooldown != 0)
                    {
                        string niceName = MSUtil.NicifyString(eqp.GetType().Name);
                        float cooldown = SS2Config.ConfigItem.Bind(niceName, "Cooldown", eqp.EquipmentDef.cooldown, "Cooldown of this equipment in seconds.").Value;
                        eqp.EquipmentDef.cooldown = cooldown;
                    }
                    AddEquipment(eqp);
                });

            base.GetEquipmentBases().ToList().ForEach(CheckEnabledStatus);

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
                ConfigEntry<bool> enabled = SS2Config.ConfigItem.Bind(niceName, "Enabled", true, "Should this item be enabled?");
                SS2Log.Info("EnableEquipments " + EnableEquipments + " | enabled? " + enabled.Value);
                if (!EnableEquipments.Value || !enabled.Value)
                {
                    //SS2Log.Info("Disabling " + niceName);
                    eqp.EquipmentDef.canDrop = false;
                    eqp.EquipmentDef.appearsInSinglePlayer = false;
                    eqp.EquipmentDef.appearsInMultiPlayer = false;
                    eqp.EquipmentDef.canBeRandomlyTriggered = false;
                    eqp.EquipmentDef.enigmaCompatible = false;
                    eqp.EquipmentDef.dropOnDeathChance = 0f;
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