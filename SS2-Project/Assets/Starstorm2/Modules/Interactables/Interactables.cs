using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Interactables : InteractableModuleBase
    {
        public static Interactables Instance { get; private set; }

        public override R2APISerializableContentPack SerializableContentPack { get; } = SS2Content.Instance.SerializableContentPack;

        [ConfigurableField(SS2Config.IDMain, ConfigSection = ": Enable All Interactables :", ConfigName = ": Enable All Interactables :", ConfigDesc = "Enables Starstorm 2's interactables. Set to false to disable interactables.")]
        public static bool EnableInteractables = true;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            if (!EnableInteractables) return;
            SS2Log.Info($"Initializing Interactables.");
            GetInteractableBases();
        }

        protected override IEnumerable<InteractableBase> GetInteractableBases()
        {
            base.GetInteractableBases()
                .Where(interactable => SS2Config.ConfigMain.Bind("Interactables", $"{interactable.Interactable}", true, "Enable/Disable this Interactable").Value)
                .ToList()
                .ForEach(interactable => AddInteractable(interactable));
            return null;
        }
    }
}