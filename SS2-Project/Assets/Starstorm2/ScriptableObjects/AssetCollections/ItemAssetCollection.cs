using RoR2;
using UnityEngine;
using System.Collections.Generic;
namespace SS2
{
    [CreateAssetMenu(fileName = "ItemAssetCollection", menuName = "Starstorm2/AssetCollections/ItemAssetCollection")]
    public class ItemAssetCollection : ExtendedAssetCollection
    {
        public List<GameObject> itemDisplayPrefabs;
        public ItemDef itemDef;
    }
}