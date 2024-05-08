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
        private AssetCollection _assetCollection;
        public override NullableRef<SerializableColorCatalogEntry> ColorIndex => null;

        public override NullableRef<SerializableColorCatalogEntry> DarkColorIndex => null;

        public override GameObject PickupDisplayVFX => _pickupDisplayVFX;
        private GameObject _pickupDisplayVFX;

        public override ItemTierDef ItemTierDef => _itemTierDef;
        private ItemTierDef _itemTierDef;

        public override void Initialize()
        {
            
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var assetRequest = SS2Assets.LoadAssetAsync<AssetCollection>("acSibylline", SS2Bundle.Items);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _assetCollection = assetRequest.Asset;

            _pickupDisplayVFX = _assetCollection.FindAsset<GameObject>("SibyllinePickupDisplayVFX");
            _itemTierDef = _assetCollection.FindAsset<ItemTierDef>("Sibylline");
            _itemTierDef.dropletDisplayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();
            yield break;
           
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.itemTierDefs.AddSingle(_itemTierDef);
        }
    }
}
