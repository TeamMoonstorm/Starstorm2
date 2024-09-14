﻿using MSU.Config;
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

        [RiskOfOptionsConfigureField(SS2Config.ID_MAIN, ConfigSectionOverride = "Typhoon", ConfigNameOverride = "Increase Team Limit", ConfigDescOverride = "Multiplies the Monster, Lunar, and Void Team maximum size by 2 when enabled. May affect performance.")]
        internal static bool IncreaseSpawnCap = true;

        private int defMonsterCap;
        public static SerializableDifficultyDef sdd;
        public override void Initialize()
        {
            sdd = DifficultyDef;
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
                TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit *= 2;
                TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit *= 2;
                TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit *= 2;
                On.RoR2.CombatDirector.Awake += CombatDirector_Awake;
            }
        }

        private void CombatDirector_Awake(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            if (IncreaseSpawnCap)
            {
                self.creditMultiplier *= 1.4f;
                self.expRewardCoefficient *= 0.8f;
                self.goldRewardCoefficient *= 0.8f;
            }
            orig(self);
        }
    }
}
