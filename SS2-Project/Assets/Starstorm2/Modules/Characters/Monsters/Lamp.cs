using MSU;
using R2API;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
namespace SS2.Monsters
{
    public sealed class Lamp : SS2Monster
    {
        public override NullableRef<MonsterCardProvider> CardProvider => null;

        public override NullableRef<DirectorAPI.DirectorCardHolder> DissonanceCard => null;

        public override NullableRef<GameObject> MasterPrefab => _masterPrefab;
        private GameObject _masterPrefab;

        public override GameObject CharacterPrefab => _characterPrefab;
        private GameObject _characterPrefab;

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
             * GameObject - "LampBody" - Monsters
             * GameObject - "LampMaster" - Monsters
             * MonsterCardProvider - ??? - Monsters
             */
            yield break;
        }
    }
}
