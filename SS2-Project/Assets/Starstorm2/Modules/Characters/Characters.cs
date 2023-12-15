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
            b.Section = "Enable Monsters";
            b.Key = "Enable Monsters";
            b.Description = "Enables Starstorm 2's monsters. Set to false to disable monsters.";
            b.ConfigFile = SS2Config.ConfigMain;
            b.CheckBoxConfig = new CheckBoxConfig
            {
                restartRequired = true,
            };
        }).DoConfigure();

        public static ConfigurableBool EnableSurvivors = SS2Config.MakeConfigurableBool(true, (b) =>
        {
            b.Section = "Enable Survivors";
            b.Key = "Enable Survivors";
            b.Description = "Enables Starstorm 2's survivors. Set to false to disable survivors.";
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

            base.GetCharacterBases()
            .Where(character =>
                {
                if (character is SurvivorBase survivor){
                    string name = MSUtil.NicifyString(character.BodyPrefab.name);
                    int ind = name.LastIndexOf("Body");
                    if(ind >= 0){
                        name = name.Substring(0, ind - 1);
                    }

                    if (!EnableSurvivors){
                        return false;
                    }

                    return SS2Config.MakeConfigurableBool(true, (b) =>
                    {
                        b.Section = "Survivors";
                        b.Key = name;
                        b.Description = "Enable/Disable this Survivor";
                        b.ConfigFile = SS2Config.ConfigMain;
                        b.CheckBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableSurvivors,
                            restartRequired = true
                        };
                    }).DoConfigure();
                }
                else if (character is MonsterBase monster){
                    string name = MSUtil.NicifyString(character.BodyPrefab.name);
                    int ind = name.LastIndexOf("Body");
                    if(ind >= 0){
                        name = name.Substring(0, ind - 1);
                    }

                    if (!EnableMonsters){
                        return false;
                    }

                    return SS2Config.MakeConfigurableBool(true, (b) =>
                    {
                        
                        b.Section = "Monsters";
                        b.Key = name;
                        b.Description = "Enable/Disable this Monster";
                        b.ConfigFile = SS2Config.ConfigMain;
                        b.CheckBoxConfig = new CheckBoxConfig
                        {
                            checkIfDisabled = () => !EnableMonsters,
                            restartRequired = true
                        };
                    }).DoConfigure();
                    }
                    else
                    {
                        SS2Log.Error("Character " + character + " was not a survivor or monster.");
                        return false;
                    }
                })
                .ToList()
                .ForEach(character => AddCharacter(character));
            return null;


            //base.GetCharacterBases()
            //    .Where(character =>
            //    {
            //        return SS2Config.MakeConfigurableBool(true, (b) =>
            //        {
            //            b.Section = "Bodies";
            //            b.Key = MSUtil.NicifyString(character.BodyPrefab.name);
            //            b.Description = "Enable/Disable this Body";
            //            b.ConfigFile = SS2Config.ConfigMain;
            //            b.CheckBoxConfig = new CheckBoxConfig
            //            {
            //                checkIfDisabled = () => !EnableMonsters,
            //                restartRequired = true
            //            };
            //        }).DoConfigure();
            //    })
            //    .ToList()
            //    .ForEach(character => AddCharacter(character));
            //return null;

        }
    }
}
