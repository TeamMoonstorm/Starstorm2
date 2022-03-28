using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Interactables : InteractableModuleBase
    {
        public static Interactables Instance { get; private set; }

        public override R2APISerializableContentPack SerializableContentPack { get; } = SS2Content.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            SS2Log.Info($"Initializing Interactables.");
            GetInteractableBases();
        }

        protected override IEnumerable<InteractableBase> GetInteractableBases()
        {
            base.GetInteractableBases()
                .Where(interactable => Starstorm.instance.Config.Bind("Starstorm 2 :: Interactables", $"{interactable.Interactable}", true, "Enable/Disable this Interactable").Value)
                .ToList()
                .ForEach(interactable => AddInteractable(interactable));
            return null;
        }
    }
}