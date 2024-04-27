using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS2
{
    /// <summary>
    /// <inheritdoc cref="IDifficultyContentPiece"/>
    /// </summary>
    public abstract class SS2Difficulty : IDifficultyContentPiece
    {
        public abstract SerializableDifficultyDef DifficultyDef { get; }

        SerializableDifficultyDef IContentPiece<SerializableDifficultyDef>.Asset => DifficultyDef;


        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public abstract IEnumerator LoadContentAsync();
        public abstract void OnRunEnd(Run run);
        public abstract void OnRunStart(Run run);
    }
}