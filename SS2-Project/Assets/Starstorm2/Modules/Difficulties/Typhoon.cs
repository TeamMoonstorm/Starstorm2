using MSU;
using MSU.Config;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine.Networking;

namespace SS2
{
    public class Typhoon : SS2Difficulty
    {
        public override SS2AssetRequest<SerializableDifficultyDef> AssetRequest => SS2Assets.LoadAssetAsync<SerializableDifficultyDef>("Typhoon", SS2Bundle.Base);
        
        private const string token = "SS2_DIFFICULTY_TYPHOON_DESC";

        // Typhoon Config
        [RiskOfOptionsConfigureField(SS2Config.ID_MAIN, configSectionOverride = "Typhoon", configNameOverride = "Increase Team Limit", configDescOverride = "Multiplies the Monster, Lunar, and Void Team maximum size when enabled. May affect performance.")]
        internal static bool IncreaseSpawnCap = true;

        [RiskOfOptionsConfigureField(SS2Config.ID_MAIN, configSectionOverride = "Typhoon", configNameOverride = "Spawn Cap Increase Percent", configDescOverride = "Percentage to increase enemy team sizes by. Only works if you have Increase Team Limit enabled. Default is 100 (doubles team size).")]
        [FormatToken(token, 0)]
        public static float spawnCapIncrease = 100f;

        [RiskOfOptionsConfigureField(SS2Config.ID_MAIN, configSectionOverride = "Typhoon", configNameOverride = "Credit Increase Percent", configDescOverride = "Percentage to increase monster director credits by. Default is 40.")]
        [FormatToken(token, 1)]
        public static float creditIncrease = 40f;

        [RiskOfOptionsConfigureField(SS2Config.ID_MAIN, configSectionOverride = "Typhoon", configNameOverride = "Experience Reward Reduction Percent", configDescOverride = "Percentage to reduce experience rewards by. Default is 20.")]
        [FormatToken(token, 2)]
        public static float expRewardReduction = 20f;

        [RiskOfOptionsConfigureField(SS2Config.ID_MAIN, configSectionOverride = "Typhoon", configNameOverride = "Gold Reward Reduction Percent", configDescOverride = "Percentage to reduce gold rewards by. Default is 20.")]
        [FormatToken(token, 3)]
        public static float goldRewardReduction = 20f;


        private int defMonsterCap;
        public static SerializableDifficultyDef sdd;
        public override void Initialize()
        {
            sdd = difficultyDef;
            EtherealBehavior.AddEtherealDifficulty(sdd.DifficultyIndex, SS2Assets.LoadAsset<SerializableDifficultyDef>("SuperTyphoon", SS2Bundle.Base));
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void OnRunEnd(Run run)
        {
            TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit = defMonsterCap;
            if (IncreaseSpawnCap)
                On.RoR2.CombatDirector.Awake -= CombatDirector_Awake;
        }

        public override void OnRunStart(Run run)
        {
            defMonsterCap = TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit;

            foreach (CharacterMaster cm in run.userMasters.Values)
                if (NetworkServer.active)
                    cm.inventory.GiveItemPermanent(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);

            if (IncreaseSpawnCap)
            {
                int newCap = (int)(defMonsterCap * (1f + spawnCapIncrease / 100f));
                TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit = newCap;
                TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit = newCap;
                TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit = newCap;
                On.RoR2.CombatDirector.Awake += CombatDirector_Awake;
            }
            run.SetEventFlag("PermanentStorms");
        }

        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            if (IncreaseSpawnCap)
            {
                self.creditMultiplier *= (1f + creditIncrease / 100f);
                self.expRewardCoefficient *= (1f - expRewardReduction / 100f);
                self.goldRewardCoefficient *= (1f - goldRewardReduction / 100f);
            }
            orig(self);
        }
    }
}
