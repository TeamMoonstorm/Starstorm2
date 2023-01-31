using R2API;
using R2API.Utils;
using RoR2;

namespace Moonstorm.Starstorm2
{
    public static class Typhoon
    {
        public static R2API.ScriptableObjects.SerializableDifficultyDef TyphoonDef { get; private set; }
        public static DifficultyIndex TyphoonIndex { get => TyphoonDef.DifficultyIndex; }

        private static int defMonsterCap;

        internal static void Init()
        {
            TyphoonDef = SS2Assets.LoadAsset<R2API.ScriptableObjects.SerializableDifficultyDef>("Typhoon", SS2Bundle.Base);
            DifficultyAPI.AddDifficulty(TyphoonDef);
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private static void Run_onRunStartGlobal(Run run)
        {
            defMonsterCap = TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit;
            if (run.selectedDifficulty == TyphoonIndex)
            {
                foreach (CharacterMaster cm in run.userMasters.Values)
                    cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);
                if (SS2Config.TyphoonIncreaseSpawnCap.Value)
                    TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit *= 2;
            }
        }

        private static void Run_onRunDestroyGlobal(Run run)
        {
            TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit = defMonsterCap;
        }
    }
}
