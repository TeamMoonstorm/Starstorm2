using RoR2;
using R2API.ScriptableObjects;
using MSU;
using System.Collections;
using RoR2.ContentManagement;
#if DEBUG
namespace SS2.Artifacts
{
    public class Havoc : SS2Artifact
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ArtifactAssetCollection>("acHavoc", SS2Bundle.Artifacts);

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override void OnArtifactDisabled()
        { 
        }

        public override void OnArtifactEnabled()
        { 
        }

        public override IEnumerator LoadContentAsync()
        {
            /*ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();
            
            helper.AddAssetToLoad<ArtifactDef>("Havoc", SS2Bundle.Artifacts);
            helper.AddAssetToLoad<ArtifactCode>("HavocCode", SS2Bundle.Artifacts);

            helper.Start();
            while (!helper.IsDone())
                yield return null;

            _artifactCode = helper.GetLoadedAsset<ArtifactCode>("HavocCode");
            _artifactDef = helper.GetLoadedAsset<ArtifactDef>("Havoc");¨*/
            yield break;
        }
    }
}
#endif
