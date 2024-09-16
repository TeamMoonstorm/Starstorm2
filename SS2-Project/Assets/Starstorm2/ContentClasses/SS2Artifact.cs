using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2
{
    /// <summary>
    /// <inheritdoc cref="IArtifactContentPiece"/>
    /// </summary>
    public abstract class SS2Artifact : IArtifactContentPiece, IContentPackModifier
    {
        public ArtifactAssetCollection assetCollection { get; private set; }
        public NullableRef<ArtifactCode> artifactCode { get; protected set; }
        public ArtifactDef artifactDef { get; protected set; }

        NullableRef<ArtifactCode> IArtifactContentPiece.ArtifactCode { get; }
        ArtifactDef IContentPiece<ArtifactDef>.asset => artifactDef;

        public abstract SS2AssetRequest assetRequest { get; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);

        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest request = assetRequest;

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            if(request.BoxedAsset is ArtifactAssetCollection collection)
            {
                assetCollection = collection;
                artifactDef = collection.artifactDef;
                artifactCode = collection.artifactCode;
            }
            else if(request.BoxedAsset is ArtifactDef artifact)
            {
                artifactDef = artifact;
            }
            else
            {
                SS2Log.Error($"Invalid AssetRequest {request.AssetName} of type {request.BoxedAsset.GetType()}");
            }
        }

        public abstract void OnArtifactDisabled();
        public abstract void OnArtifactEnabled();

        public void ModifyContentPack(ContentPack contentPack)
        {
            if (assetCollection)
                contentPack.AddContentFromAssetCollection(assetCollection);
        }
    }
}