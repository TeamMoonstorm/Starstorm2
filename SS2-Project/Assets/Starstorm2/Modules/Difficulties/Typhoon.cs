using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2
{
    public static class Typhoon
    {
        public static R2API.ScriptableObjects.SerializableDifficultyDef TyphoonDef { get; private set; }
        public static DifficultyIndex TyphoonIndex { get => TyphoonDef.DifficultyIndex; }

        private static int defMonsterCap;

        [RooConfigurableField(SS2Config.IDMain, ConfigSection = "Typhoon", ConfigName = "Increase Team Limit", ConfigDesc = "Multiplies the Monster, Lunar, and Void Team maximum size by 2 when enabled. May affect performance.")]
        internal static bool IncreaseSpawnCap = true;

        internal static void Init()
        {
            TyphoonDef = SS2Assets.LoadAsset<R2API.ScriptableObjects.SerializableDifficultyDef>("Typhoon", SS2Bundle.Base);
            DifficultyAPI.AddDifficulty(TyphoonDef);
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        public static void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            if (IncreaseSpawnCap)
            {
                self.creditMultiplier *= 1.25f;
                self.expRewardCoefficient *= 0.8f;
                self.goldRewardCoefficient *= 0.8f;
                //Debug.Log("creditMultiplier = " + self.creditMultiplier);
                //Debug.Log("expRewardCoefficient = " + self.expRewardCoefficient);
                //Debug.Log("goldRewardCoefficient = " + self.goldRewardCoefficient);
            }
            orig(self);
        }

        private static void Run_onRunStartGlobal(Run run)
        {
            defMonsterCap = TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit;
            if (run.selectedDifficulty == TyphoonIndex)
            {
                foreach (CharacterMaster cm in run.userMasters.Values)
                    cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);
                if (IncreaseSpawnCap)
                {
                    TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit *= 2;
                    TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit *= 2;
                    TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit *= 2;
                    On.RoR2.CombatDirector.Awake += CombatDirector_Awake;
                }
            }
            //On.RoR2.CombatDirector.Awake += CombatDirector_Awake_Test;
        }

        private static void Run_onRunDestroyGlobal(Run run)
        {
            TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit = defMonsterCap;
            //On.RoR2.CombatDirector.Awake -= CombatDirector_Awake_Test;
            if (IncreaseSpawnCap)
                On.RoR2.CombatDirector.Awake -= CombatDirector_Awake;
        }
    }
}
