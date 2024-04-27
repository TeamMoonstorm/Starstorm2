using MSU;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2.ContentManagement;

namespace SS2
{
    /// <summary>
    /// <inheritdoc cref="ISurvivorContentPiece"/>
    /// </summary>
    public abstract class SS2Survivor : ISurvivorContentPiece
    {
        public abstract SurvivorDef SurvivorDef { get; }
        public abstract NullableRef<GameObject> MasterPrefab { get; }
        CharacterBody IGameObjectContentPiece<CharacterBody>.Component => CharacterPrefab.GetComponent<CharacterBody>();
        GameObject IContentPiece<GameObject>.Asset => CharacterPrefab;
        public abstract GameObject CharacterPrefab { get; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public abstract IEnumerator LoadContentAsync();
    }
}