using MSU;
using R2API.ScriptableObjects;
using RoR2;
using System.Collections;
using UnityEngine.SceneManagement;
using RoR2.ContentManagement;
using R2API;
using System.Collections.Generic;

namespace SS2.Artifacts
{
#if DEBUG
    public class Adversity : SS2Artifact
    {
        public override SS2AssetRequest assetRequest => SS2Assets.LoadAssetAsync<ArtifactAssetCollection>("acAdversity", SS2Bundle.Artifacts);

        public static bool shouldUpgradeTP;
        public static float timer;
        public override void Initialize(){}

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void OnArtifactDisabled(){}
        public override void OnArtifactEnabled(){}
    }
#endif
}
