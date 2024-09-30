using RoR2;
using UnityEngine;
using System.Collections.Generic;
using RoR2.Artifacts;
using R2API.ScriptableObjects;
namespace SS2
{
    [CreateAssetMenu(fileName = "ArtifactAssetCollection", menuName = "Starstorm2/AssetCollections/ArtifactAssetCollection")]
    public class ArtifactAssetCollection : ExtendedAssetCollection
    {
        public ArtifactCode artifactCode;
        public ArtifactDef artifactDef;
    }
}