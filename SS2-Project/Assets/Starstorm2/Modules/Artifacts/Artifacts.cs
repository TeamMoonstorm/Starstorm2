using MSU;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using System.Linq;
namespace SS2.Modules
{
    
    public sealed class Artifacts
    {
        [AsyncAssetLoad]
        public static System.Collections.IEnumerator Initialize()
        {

            SS2AssetRequest request = SS2Assets.LoadAssetAsync<ArtifactCompoundDef>("acdStar", SS2Bundle.Artifacts);
            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            var compound = request.BoxedAsset;
            //compound.decalMaterial.shader = Resources.Load<ArtifactCompoundDef>("artifactcompound/acdCircle").decalMaterial.shader;
            ArtifactCodeAPI.AddCompound(compound as ArtifactCompoundDef);
        }

    }
}