using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;

namespace SS2
{
    public class Deluge : SS2Difficulty
    {
        public override SerializableDifficultyDef DifficultyDef => _difficultyDef;
        private SerializableDifficultyDef _difficultyDef;

        private int defMonsterCap;
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * SerializableDifficultyDef - "Deluge" Base
             */
            yield break;
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
