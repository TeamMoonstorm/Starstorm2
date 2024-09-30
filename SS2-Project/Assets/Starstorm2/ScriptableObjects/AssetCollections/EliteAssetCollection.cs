using RoR2;
using UnityEngine;
using System.Collections.Generic;
namespace SS2
{
    [CreateAssetMenu(fileName = "EliteAssetCollection", menuName = "Starstorm2/AssetCollections/EliteAssetCollection")]
    public class EliteAssetCollection : EquipmentAssetCollection
    {
        public List<EliteDef> eliteDefs;
    }
}