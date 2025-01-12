using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
namespace SS2.Monsters
{
    public sealed class LampBoss : SS2Monster
    {
        [RiskOfOptionsConfigureField(SS2Config.ID_MONSTER, configSectionOverride = "Lamp Boss", configNameOverride = "Stage Allow List", configDescOverride = "What stages the monster will show up on. List of stage names can be found at https://github.com/risk-of-thunder/R2Wiki/wiki/List-of-scene-names")]
        internal static string allowStageList = "";

        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acLampBoss", SS2Bundle.Monsters);

        private static BodyIndex BodyIndex;
        public override void Initialize()
        {
            RoR2Application.onLoad += () => BodyIndex = BodyCatalog.FindBodyIndex("LampBossBody");
            R2API.RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;

            ExpandSpawnableStages(allowStageList);
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(SS2Content.Buffs.bdLampBuff) && sender.bodyIndex != BodyIndex)
            {
                args.primaryCooldownMultAdd += 0.5f;
                args.secondaryCooldownMultAdd += 0.25f;
                args.damageMultAdd += 0.2f;
                args.moveSpeedMultAdd += 1.5f;
            }
        }


        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        [Serializable]
        public class MoonPhaseData
        {
            public PhaseData[] phasedata;
        }

        [Serializable]
        public class PhaseData
        {
            public string phase;
        }
    }
}
