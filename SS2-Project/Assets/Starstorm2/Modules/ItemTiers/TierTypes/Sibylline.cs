using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace SS2.ItemTiers
{
    public class Sibylline : SS2ItemTier, IContentPackModifier
    {
        public override SS2AssetRequest<ItemTierAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<ItemTierAssetCollection>("acSibylline", SS2Bundle.Base);

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var enumerator = base.LoadContentAsync();
            while (!enumerator.IsDone())
                yield return null;

            var request = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab");
            while (!request.IsDone)
                yield return null;

            ItemTierDef.dropletDisplayPrefab = request.Result;
            yield break;
        }
    }
}
