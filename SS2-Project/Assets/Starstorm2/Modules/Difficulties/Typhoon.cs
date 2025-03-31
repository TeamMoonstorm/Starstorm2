using MSU;
using MSU.Config;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine.Networking;
using static AkMIDIEvent;

namespace SS2
{
    public class Typhoon : SS2Difficulty
    {
        public override SS2AssetRequest<SerializableDifficultyDef> AssetRequest => SS2Assets.LoadAssetAsync<SerializableDifficultyDef>("Typhoon", SS2Bundle.Base);
        
        // Typhoon Config
        [RiskOfOptionsConfigureField(SS2Config.ID_MAIN, configSectionOverride = "Typhoon", configNameOverride = "Increase Team Limit", configDescOverride = "Multiplies the Monster, Lunar, and Void Team maximum size by 2 (or the configured value) when enabled. May affect performance.")]
        internal static bool IncreaseSpawnCap = true;

        [RiskOfOptionsConfigureField(SS2Config.ID_MAIN, configSectionOverride = "Typhoon", configNameOverride = "Spawn Cap Multiplier", configDescOverride = "The spawn cap multiplier to increase enemy team sizes by. Only works if you have Increase Team Limit enabled. Default is 2.")]
        public static int spawnCapMultiplier = 2;

        [RiskOfOptionsConfigureField(SS2Config.ID_MAIN, configSectionOverride = "Typhoon", configNameOverride = "Credit Multiplier",  configDescOverride = "The credit multipler the monster director has for spawning enemies.")]
        public static float creditMultiplierVal = 1.4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_MAIN, configSectionOverride = "Typhoon", configNameOverride = "Experience Reward Multipler", configDescOverride = "The experience reward multipler when defeating an enemy.")]
        public static float expRewardCoefficientVal = 0.8f;

        [RiskOfOptionsConfigureField(SS2Config.ID_MAIN, configSectionOverride = "Typhoon", configNameOverride = "Gold Reward Multipler", configDescOverride = "The gold reward multipler when defeating an enemy.")]
        public static float goldRewardCoefficientVal = 0.8f;


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
                    cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);
            if (IncreaseSpawnCap)
            {
                TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit *= spawnCapMultiplier;
                TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit *= spawnCapMultiplier;
                TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit *= spawnCapMultiplier;
                On.RoR2.CombatDirector.Awake += CombatDirector_Awake;
            }
            run.SetEventFlag("PermanentStorms");
        }

        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            if (IncreaseSpawnCap)
            {
                self.creditMultiplier *= creditMultiplierVal;
                self.expRewardCoefficient *= expRewardCoefficientVal;
                self.goldRewardCoefficient *= goldRewardCoefficientVal;
            }
            orig(self);
        }
    }
}
