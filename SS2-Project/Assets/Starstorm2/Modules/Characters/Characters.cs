using Moonstorm.Config;
using R2API.ScriptableObjects;
using RiskOfOptions.OptionConfigs;
using System.Collections.Generic;
using System.Linq;

namespace Moonstorm.Starstorm2.Modules
{
    public sealed class Characters : CharacterModuleBase
    {
        public static Characters Instance { get; set; }

        public override R2APISerializableContentPack SerializableContentPack => SS2Content.Instance.SerializableContentPack;

        public static ConfigurableBool EnableMonsters = SS2Config.MakeConfigurableBool(true, (b) =>
        {
            b.Section = "Enable All Bodies";
            b.Key = "Enable All Bodies";
            b.Description = "Enables Starstorm 2's Bodies. Set to false to disable survivors and monsters.";
            b.ConfigFile = SS2Config.ConfigMain;
            b.CheckBoxConfig = new CheckBoxConfig
            {
                restartRequired = true,
            };
        }).DoConfigure();

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            SS2Log.Info($"Initializing Bodies.");
            GetCharacterBases();
        }

        protected override IEnumerable<CharacterBase> GetCharacterBases()
        {
            //base.GetCharacterBases()
            //    .ToList()
            //    .ForEach(character => AddCharacter(character));
            //return null;

            base.GetCharacterBases()
                .Where(character =>
                {
                    return SS2Config.MakeConfigurableBool(true, (b) =>
                    {
                        b.Section = "Bodies";
                        b.Key = MSUtil.NicifyString(character.BodyPrefab.name);
                        b.Description = "Enable/Disable this Body";
                        b.ConfigFile = SS2Config.ConfigMain;
                        b.CheckBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableMonsters,
                            restartRequired = true
                        };
                    }).DoConfigure();
                })
                .ToList()
                .ForEach(character => AddCharacter(character));
            return null;

        }
    }
}
