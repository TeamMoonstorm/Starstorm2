using MSU;
using R2API;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SS2
{
    public abstract class SS2Monster : IMonsterContentPiece
    {
        public abstract NullableRef<MonsterCardProvider> CardProvider { get; }
        public abstract NullableRef<DirectorAPI.DirectorCardHolder> DissonanceCard { get; }
        public abstract NullableRef<GameObject> MasterPrefab { get; }
        CharacterBody IGameObjectContentPiece<CharacterBody>.Component => CharacterPrefab.GetComponent<CharacterBody>();
        GameObject IContentPiece<GameObject>.Asset => CharacterPrefab;
        public abstract GameObject CharacterPrefab { get; }

        public abstract void Initialize();
        public abstract bool IsAvailable();
        public abstract IEnumerator LoadContentAsync();
    }
}
