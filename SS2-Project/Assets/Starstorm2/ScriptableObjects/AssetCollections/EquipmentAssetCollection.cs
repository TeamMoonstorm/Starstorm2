using RoR2;
using UnityEngine;
using System.Collections.Generic;
namespace SS2
{
    [CreateAssetMenu(fileName = "EquipmentAssetCollection", menuName = "Starstorm2/AssetCollections/EquipmentAssetCollection")]
    public class EquipmentAssetCollection : ExtendedAssetCollection
    {
        public List<GameObject> itemDisplayPrefabs;
        public EquipmentDef equipmentDef;
    }
}