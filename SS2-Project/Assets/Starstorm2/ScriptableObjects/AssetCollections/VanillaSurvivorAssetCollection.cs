using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SS2
{
    [CreateAssetMenu(fileName = "VanillaSurvivorAssetCollection", menuName = "Starstorm2/AssetCollections/VanillaSurvivorAssetCollection")]
    public class VanillaSurvivorAssetCollection : ExtendedAssetCollection
    {
        public AssetReferenceT<SurvivorDef> associatedSurvivorDef;
    }
}
