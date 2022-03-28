using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Scenes : SceneModuleBase
    {
        public static Scenes Instance { get; private set; }
        public override R2APISerializableContentPack SerializableContentPack { get; } = SS2Content.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            SS2Log.Info($"Initializing Scenes.");
            GetSceneBases();
        }

        public override IEnumerable<SceneBase> GetSceneBases()
        {
            base.GetSceneBases()
                .ToList()
                .ForEach(scene => AddScene(scene));

            return null;
        }
    }
}
