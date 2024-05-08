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
        public SerializableDifficultyDef DifficultyDef;

        SerializableDifficultyDef IContentPiece<SerializableDifficultyDef>.Asset => DifficultyDef;

        public abstract SS2AssetRequest<SerializableDifficultyDef> AssetRequest { get; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            var assetRequest = AssetRequest;

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            DifficultyDef = assetRequest.Asset;
            yield break;
        }
        public abstract void OnRunEnd(Run run);
        public abstract void OnRunStart(Run run);
    }
}