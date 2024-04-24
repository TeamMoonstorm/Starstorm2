using R2API.ScriptableObjects;
using RiskOfOptions.OptionConfigs;
using System.Collections.Generic;
using System.Linq;
using MSU;
using UnityEngine;
using RoR2.ContentManagement;
using MSU.Config;
using RoR2;
using BepInEx;

namespace SS2.Modules
{
    public sealed class Characters : IContentPieceProvider<GameObject>
    {
        public ContentPack ContentPack => _contentPack;
        private ContentPack _contentPack;

        public static ConfiguredBool EnableMonsters = SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
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

        public static ConfiguredBool EnableSurvivors = SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
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

        private IEnumerable<IContentPiece<GameObject>> _unfilteredCharacters;

        public IContentPiece<GameObject>[] GetContents()
        {
            return _unfilteredCharacters.Where(PassesFilter).ToArray();
        }

        IContentPiece[] IContentPieceProvider.GetContents()
        {
            return _unfilteredCharacters.Where(PassesFilter).ToArray();
        }

        private bool PassesFilter(IContentPiece<GameObject> piece)
        {
            if(!(piece is ICharacterContentPiece character))
            {
                return false;
            }

            if (!character.IsAvailable(ContentPack))
                return false;

            if (character is SS2Survivor starstormSurvivor)
            {
                if (!EnableSurvivors)
                    return false;

                string name = MSUtil.NicifyString(starstormSurvivor.CharacterPrefab.name);
                int ind = name.LastIndexOf("Body");
                if (ind >= 0)
                {
                    name = name.Substring(0, ind - 1);
                }

                return SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
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

            if(character is SS2Monster starstormMonster)
            {
                if (!EnableMonsters)
                    return false;

                string name = MSUtil.NicifyString(starstormMonster.CharacterPrefab.name);
                int ind = name.LastIndexOf("Body");
                if (ind >= 0)
                {
                    name = name.Substring(0, ind - 1);
                }

                return SS2Config.ConfigFactory.MakeConfiguredBool(true, (b) =>
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

            return false;
        }

        internal Characters(ContentPack contentPack, BaseUnityPlugin plugin)
        {
            _contentPack = contentPack;
            _unfilteredCharacters = ContentUtil.AnalyzeForGameObjectContentPieces<CharacterBody>(plugin);
        }
    }
}
