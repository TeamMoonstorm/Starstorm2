using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace SS2.ItemTiers
{
    public class Sibylline : ItemTierBase
    {
        public override ItemTierDef ItemTierDef => SS2Assets.LoadAsset<ItemTierDef>("Sibylline", SS2Bundle.Items);

        public override GameObject PickupDisplayVFX => SS2Assets.LoadAsset<GameObject>("SibyllinePickupDisplayVFX", SS2Bundle.Items);

        public override void Initialize()
        {
            base.Initialize();
            //ItemTierDef.highlightPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();
            ItemTierDef.dropletDisplayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();
        }
    }
}
