using R2API;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Moonstorm.Starstorm2
{
    public static class Deluge
    {
        public static R2API.ScriptableObjects.SerializableDifficultyDef DelugeDef { get; private set; }
        public static DifficultyIndex DelugeIndex { get => DelugeDef.DifficultyIndex; }

        private static int defMonsterCap;
        private static RuleChoiceDef rcd;

        internal static void Init()
        {
            DelugeDef = SS2Assets.LoadAsset<R2API.ScriptableObjects.SerializableDifficultyDef>("Deluge", SS2Bundle.Base);
            DifficultyAPI.AddDifficulty(DelugeDef);
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
            Run.onRunStartGlobal += Run_onRunStartGlobal;
        }

        private static void Run_onRunStartGlobal(Run run)
        {
            defMonsterCap = TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit;
        }

        private static void Run_onRunDestroyGlobal(Run run)
        {
            TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit = defMonsterCap;
        }
    }
}
