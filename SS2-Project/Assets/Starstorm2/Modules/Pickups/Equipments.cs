using BepInEx.Configuration;
using Moonstorm.Config;
using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using RiskOfOptions.OptionConfigs;
using static System.Collections.Specialized.BitVector32;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Equipments : EquipmentModuleBase
    {
        public static Equipments Instance { get; private set; }
        public override R2APISerializableContentPack SerializableContentPack => SS2Content.Instance.SerializableContentPack;

        public static ConfigurableBool EnableEquipments = SS2Config.MakeConfigurableBool(true, (b) =>
        {
            b.Section = "Enable All Equipments";
            b.Key = "Enable All Equipments";
            b.Description = "Enables Starstorm 2's equipments. Set to false to disable all equipments.";
            b.ConfigFile = SS2Config.ConfigItem;
            b.CheckBoxConfig = new CheckBoxConfig
            {
                restartRequired = true,
            };
        }).DoConfigure();

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
                .ForEach(eqp => {
                    if (eqp.EquipmentDef.cooldown != 0)
                    {
                        string niceName = MSUtil.NicifyString(eqp.GetType().Name);
                        var cfg = SS2Config.MakeConfigurableFloat(eqp.EquipmentDef.cooldown, (f) =>
                        {
                            f.Section = niceName;
                            f.Key = "Cooldown";
                            f.Description = "Cooldown of this equipment in seconds";
                            f.ConfigFile = SS2Config.ConfigItem;
                            f.UseStepSlider = false;
                            f.OnConfigChanged += (f1) => eqp.EquipmentDef.cooldown = f1;
                            f.SliderConfig = new SliderConfig
                            {
                                checkIfDisabled = () => !EnableEquipments,
                                min = 0,
                                max = 600,
                                formatString = "{0:0.0}",
                            };
                        });
                        cfg.DoConfigure();
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

        private void CheckEnabledStatus(EquipmentBase eqp)
        {
            string niceName = MSUtil.NicifyString(eqp.GetType().Name);
            if (!(eqp.EquipmentDef.dropOnDeathChance > 0 || eqp.EquipmentDef.passiveBuffDef || niceName.ToLower().Contains("affix")))
            {
                //string niceName = MSUtil.NicifyString(eqp.GetType().Name);
                var cfg = SS2Config.MakeConfigurableBool(true, b =>
                {
                    b.Section = niceName;
                    b.Key = "Enabled";
                    b.Description = "Should this equipment be enabled?";
                    b.ConfigFile = SS2Config.ConfigItem;
                    b.CheckBoxConfig = new CheckBoxConfig
                    {
                        checkIfDisabled = () => !EnableEquipments,
                        restartRequired = true
                    };
                }).DoConfigure();
#if DEBUG
                SS2Log.Info("EnableEquipments " + EnableEquipments + " | enabled? " + cfg);
#endif
                if (!EnableEquipments || !cfg)
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