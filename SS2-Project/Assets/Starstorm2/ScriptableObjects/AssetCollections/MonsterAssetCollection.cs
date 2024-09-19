using RoR2;
using UnityEngine;
using MSU;
using System.Collections.Generic;
using R2API.AddressReferencedAssets;

namespace SS2
{
    [CreateAssetMenu(fileName = "MonsterAssetCollection", menuName = "Starstorm2/AssetCollections/MonsterAssetCollection")]
    public class MonsterAssetCollection : BodyAssetCollection
    {
        public MonsterCardProvider monsterCardProvider;
    }
}