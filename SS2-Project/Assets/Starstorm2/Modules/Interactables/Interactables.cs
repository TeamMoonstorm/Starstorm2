using Moonstorm.Config;
using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using RiskOfOptions.OptionConfigs;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Interactables : InteractableModuleBase
    {
        public static Interactables Instance { get; private set; }

        public override R2APISerializableContentPack SerializableContentPack { get; } = SS2Content.Instance.SerializableContentPack;

        public static ConfigurableBool EnableInteractables = new ConfigurableBool(true)
        {
            Section = "Enable All Interactables",
            Key = "Enable All Interactables",
            Description = "Enables Starstorm 2's interactables. Set to false to disable interactables.",
            CheckBoxConfig = new CheckBoxConfig
            {
                restartRequired = true,
            }
        };

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            EnableInteractables.SetConfigFile(SS2Config.ConfigMain).DoConfigure();
            if (!EnableInteractables) return;
            SS2Log.Info($"Initializing Interactables.");
            GetInteractableBases();
        }

        protected override IEnumerable<InteractableBase> GetInteractableBases()
        {
            base.GetInteractableBases()
                .Where(interactable =>
                {
                    return new ConfigurableBool(true)
                    {
                        Section = "Interactables",
                        Key = interactable.Interactable.ToString(),
                        Description = "Enable/Disable this Interactable",
                        ConfigFile = SS2Config.ConfigMain,
                        CheckBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableInteractables,
                            restartRequired = true
                        }
                    }.DoConfigure();
                })
                .ToList()
                .ForEach(interactable => AddInteractable(interactable));
            return null;
        }
    }
}