using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using SS2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Starstorm2.ContentClasses
{
    public abstract class SS2VanillaSurvivor : IContentPiece, IContentPackModifier
    {
        public abstract SS2AssetRequest<AssetCollection> AssetRequest { get; }

        public AssetCollection survivorAssetCollection;
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            var assetRequest = AssetRequest;

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
            {
                yield return null;
            }

            survivorAssetCollection = assetRequest.Asset;

             yield break;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(survivorAssetCollection);
        }
    }
}
