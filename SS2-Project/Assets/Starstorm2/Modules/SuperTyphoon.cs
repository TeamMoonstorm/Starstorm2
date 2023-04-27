using R2API;
using R2API.Utils;
using RoR2;

namespace Moonstorm.Starstorm2
{
    public static class SuperTyphoon
    {
        public static R2API.ScriptableObjects.SerializableDifficultyDef SuperTyphoonDef { get; private set; }
        public static DifficultyIndex SuperTyphoonIndex { get => SuperTyphoonDef.DifficultyIndex; }

        private static int defMonsterCap;

        internal static void Init()
        {
            SuperTyphoonDef = SS2Assets.LoadAsset<R2API.ScriptableObjects.SerializableDifficultyDef>("SuperTyphoon", SS2Bundle.Base);
            DifficultyAPI.AddDifficulty(SuperTyphoonDef);
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private static void Run_onRunStartGlobal(Run run)
        {
            defMonsterCap = TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit;
            if (run.selectedDifficulty == SuperTyphoonIndex)
            {
                foreach (CharacterMaster cm in run.userMasters.Values)
                    cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);
                if (SS2Config.TyphoonIncreaseSpawnCap.Value)
                {
                    TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit *= 3;
                    TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit *= 3;
                    TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit *= 3;
                }
            }
        }

        private static void Run_onRunDestroyGlobal(Run run)
        {
            TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit = defMonsterCap;
        }
    }
}
