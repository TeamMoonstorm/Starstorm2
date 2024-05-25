using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;

namespace SS2
{
    public class Tempest : SS2Difficulty
    {
        public override SS2AssetRequest<SerializableDifficultyDef> AssetRequest => SS2Assets.LoadAssetAsync<SerializableDifficultyDef>("Tempest", SS2Bundle.Base);

        private int defMonsterCap;
        private RuleChoiceDef rcd;
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
        }

        public override void OnRunStart(Run run)
        {
            defMonsterCap = TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit;
        }
    }
}
