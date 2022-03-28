using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Characters : CharacterModuleBase
    {
        public static Characters Instance { get; set; }

        public override R2APISerializableContentPack SerializableContentPack => SS2Content.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            SS2Log.Info($"Initializing Bodies.");
            GetCharacterBases();
        }

        protected override IEnumerable<CharacterBase> GetCharacterBases()
        {
            base.GetCharacterBases()
                .ToList()
                .ForEach(character => AddCharacter(character));
            return null;
        }
    }
}
