using MSU;
using R2API;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Monsters
{
#if DEBUG
    public sealed class Agarthan : SS2Monster
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
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * GameObject - "AgarthanBody" - Indev
             * GameObject - "AgarthanMaster" - Indev
             */
            yield break;
        }
    }
#endif
}
