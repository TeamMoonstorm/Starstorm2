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
using UnityEngine.AddressableAssets;

namespace SS2
{
    public abstract class SS2VanillaSurvivor : IVanillaSurvivorContentPiece, IContentPackModifier
    {
        public VanillaSurvivorAssetCollection assetCollection { get; private set; }
        public SurvivorDef survivorDef { get; set; }
        public abstract SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest { get; }

        public abstract void Initialize();

        public virtual IEnumerator InitializeAsync()
        {
            var coroutine = assetCollection.InitializeSkinDefs();

            while (!coroutine.IsDone())
                yield return null;

            yield break;
        }

        public abstract bool IsAvailable(ContentPack contentPack);

        public IEnumerator LoadContentAsync()
        {
            var assetRequest = this.assetRequest;

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            assetCollection = assetRequest.Asset;

            var request = Addressables.LoadAssetAsync<SurvivorDef>(assetCollection.survivorDefAddress);
            while (!request.IsDone)
                yield return null;

            survivorDef = request.Result;
        }

        public abstract void ModifyContentPack(ContentPack contentPack);
    }
}
