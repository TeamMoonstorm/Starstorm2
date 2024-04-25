using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace SS2.ItemTiers
{
    public class Sibylline : SS2ItemTier
    {
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
            /*
             * ItemTierDef - "Sibylline" - Items
             * GameOBject - "SibyllinePickupDisplayVFX" - Items
             * Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion(); (Droplet Display Prefab for ItemTierDef)
             */
            yield break;
        }
    }
}
